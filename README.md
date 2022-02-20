# pipefilter

[![CC BY-NC-SA 4.0][cc-by-nc-sa-shield]][cc-by-nc-sa] ![project status][status-shield]

This work is licensed under a
[Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License][cc-by-nc-sa].

[cc-by-nc-sa]: LICENSE
[cc-by-nc-sa-shield]: https://img.shields.io/badge/License-CC%20BY--NC--SA%204.0-informational.svg
[status-shield]: https://img.shields.io/badge/status-maintenance%20mode-orange

# config format

```json
{
    "verbose": false,
    "name": "^\\d+",
    "namePattern": "{0}_out.log",
    "logTime": true,
    "regex": [
        {
            "from": "one",
            "to": "two"
        },
        {
            "from": "three",
            "delete": true
        }
    ]
}
```

  - **verbose**: writes debugging info to stderr 
  - **name**: naming pattern for separate logfiles
  - **namePattern**: the pattern for the filename with ```{0}``` as the replacement position
  - **logTime**: shall a timestamp be printed at the beginning of the line
  - **regex**: multiple regular expressions to filter on the input lines
    - **from**: source pattern
    - **to**: in case of "to", the replacement pattern for "from"
    - **delete**: in case of "delete", remove the line from output