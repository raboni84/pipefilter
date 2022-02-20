using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

class Program
{
    static void Main(string[] args)
    {
        Filter filter;
        args = GetCfg(args, out filter);

        StreamReader sr;
        StreamWriter sw, se;
        GetInOutErr(args, filter.Verbose, out sr, out sw, out se);
        se.WriteLine($"in: {sr.BaseStream.CanRead}, out: {sw.BaseStream.CanWrite}, err: {se.BaseStream.CanWrite}");

        List<Regex> rex = new List<Regex>();
        Regex name = null;
        if (filter.Name != null)
            name = new Regex(filter.Name);
        foreach (var elem in filter.Regex)
        {
            se.WriteLine($"regex: {elem.From}");
            rex.Add(new Regex(elem.From, RegexOptions.Compiled | RegexOptions.Multiline));
        }

        Dictionary<string, StreamWriter> redir = new Dictionary<string, StreamWriter>();
        string line;
        while ((line = sr.ReadLine()) != null)
        {
            se.WriteLine($"line: {line}");
            StreamWriter next = GetNext(filter, name, sw, se, redir, line);

            for (int i = 0; line != null && i < rex.Count; i++)
            {
                Match match = rex[i].Match(line);
                if (match.Success && !string.IsNullOrEmpty(match.Value))
                {
                    se.WriteLine($"match: {match.Value}");
                    if (filter.Regex[i].To != null)
                    {
                        se.WriteLine($"replace line");
                        line = rex[i].Replace(line, filter.Regex[i].To);
                    }
                    else
                    {
                        se.WriteLine($"remove line");
                        line = null;
                    }
                    continue;
                }
            }
            if (line != null)
            {
                se.WriteLine($"line output");
                if (filter.LogTime)
                    line = $"{DateTimeOffset.Now.ToString("dd MMM yyyy HH:mm:ss.ffff zz")}: {line}";
                next.WriteLine(line);
            }
        }

        sr.Close();
        sw.Close();
        foreach (var elem in redir)
            elem.Value.Close();
    }

    private static string[] GetCfg(string[] args, out Filter filter)
    {
        string data = null;
        if (args[0] == "-c" || args[0] == "--config")
        {
            data = File.ReadAllText(args[1]);
            args = args.Skip(2).ToArray();
        }
        if (data == null)
        {
            if (!File.Exists("filter.json"))
            {
                filter = new Filter
                {
                    Regex = new FilterValue[0]
                };
                return args;
            }
            data = File.ReadAllText("filter.json");
        }
        filter = JsonSerializer.Deserialize<Filter>(data, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        return args;
    }

    private static void GetInOutErr(string[] args, bool verbose, out StreamReader sr, out StreamWriter sw, out StreamWriter se)
    {
        string arg = args.FirstOrDefault();
        if (arg == null || arg == "-")
            sr = new StreamReader(Console.OpenStandardInput(), new UTF8Encoding(false));
        else
            sr = new StreamReader(arg, new UTF8Encoding(false));
        sw = new StreamWriter(Console.OpenStandardOutput(), new UTF8Encoding(false));
        sw.AutoFlush = true;
        if (verbose)
            se = new StreamWriter(Console.OpenStandardError(), new UTF8Encoding(false));
        else
            se = new StreamWriter(Stream.Null);
        se.AutoFlush = true;
    }

    private static StreamWriter GetNext(Filter filter, Regex name, StreamWriter sw, StreamWriter se, Dictionary<string, StreamWriter> redir, string line)
    {
        StreamWriter next = null;
        if (name != null)
        {
            Match match = name.Match(line);
            if (match != null && match.Success)
            {
                se.WriteLine($"named sw: {match.Value}");
                if (!redir.TryGetValue(match.Value, out next))
                {
                    next = new StreamWriter(string.Format(filter.NamePattern, match.Value));
                    redir.Add(match.Value, next);
                }
            }
        }
        if (next == null)
        {
            se.WriteLine($"default sw");
            next = sw;
        }
        return next;
    }
}