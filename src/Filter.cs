class Filter
{
    public bool Verbose { get; set; }
    public string Name { get; set; }
    public string NamePattern { get; set; }
    public bool LogTime { get; set; }
    public FilterValue[] Regex { get; set; }
}

class FilterValue
{
    public string From { get; set; }
    public string To { get; set; }
    public bool Delete { get; set; }
}