using System.Text;
using BinSite.Social.Models;

namespace BinSite.Social.Services;

/// <summary>Parses bounded delimited text while retaining source lines and quoted field contents.</summary>
public static class CatalogStudioCsv
{
    public const int MaximumRows = 2000;
    public const int MaximumColumns = 40;
    public const int MaximumCharacters = 5_000_000;

    /// <summary>Detects an unquoted comma, semicolon or tab in the first logical record.</summary>
    public static char DetectDelimiter(string source)
    {
        var counts = new Dictionary<char, int> { [','] = 0, [';'] = 0, ['\t'] = 0 };
        var quoted = false;
        for (var index = 0; index < source.Length; index++)
        {
            var character = source[index];
            if (character == '"')
            {
                if (quoted && index + 1 < source.Length && source[index + 1] == '"') index++;
                else quoted = !quoted;
            }
            else if (!quoted && character is '\r' or '\n') break;
            else if (!quoted && counts.ContainsKey(character)) counts[character]++;
        }
        return counts.OrderByDescending(pair => pair.Value).First().Key;
    }

    /// <summary>Reads header-based CSV with escaped quotes, quoted delimiters and quoted CR/LF fields.</summary>
    public static CatalogStudioCsvTable Parse(string source, char? delimiter = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (source.Length > MaximumCharacters) throw new FormatException("CSV exceeds the five-million-character limit.");
        var separator = delimiter ?? DetectDelimiter(source);
        if (separator is not (',' or ';' or '\t')) throw new FormatException("Choose comma, semicolon or tab as the delimiter.");
        if (source.StartsWith('\uFEFF')) source = source[1..];
        var records = new List<CatalogStudioCsvRow>();
        var issues = new List<CatalogStudioIssue>();
        var fields = new List<string>();
        var field = new StringBuilder();
        var line = 1;
        var rowLine = 1;
        var quoted = false;
        var closedQuote = false;

        void EndField()
        {
            fields.Add(field.ToString());
            field.Clear();
            closedQuote = false;
            if (fields.Count > MaximumColumns) throw new FormatException($"Line {rowLine}: more than {MaximumColumns} columns.");
        }

        void EndRow()
        {
            EndField();
            if (fields.Any(value => !string.IsNullOrWhiteSpace(value))) records.Add(new(rowLine, fields.ToArray()));
            fields.Clear();
            if (records.Count > MaximumRows + 1) throw new FormatException($"CSV contains more than {MaximumRows} product rows.");
        }

        for (var index = 0; index < source.Length; index++)
        {
            var character = source[index];
            if (quoted)
            {
                if (character == '"')
                {
                    if (index + 1 < source.Length && source[index + 1] == '"') { field.Append('"'); index++; }
                    else { quoted = false; closedQuote = true; }
                }
                else if (character is '\r' or '\n')
                {
                    if (character == '\r' && index + 1 < source.Length && source[index + 1] == '\n') index++;
                    field.Append('\n');
                    line++;
                }
                else field.Append(character);
            }
            else if (character == separator) EndField();
            else if (character is '\r' or '\n')
            {
                EndRow();
                if (character == '\r' && index + 1 < source.Length && source[index + 1] == '\n') index++;
                line++;
                rowLine = line;
            }
            else if (character == '"')
            {
                if (field.Length == 0 && !closedQuote) quoted = true;
                else issues.Add(new(rowLine, "CSV", "A quote appeared inside an unquoted field.", true));
            }
            else if (closedQuote && !char.IsWhiteSpace(character))
                issues.Add(new(rowLine, "CSV", "Unexpected text after a closing quote.", true));
            else if (!closedQuote) field.Append(character);
            if (field.Length > 5000) throw new FormatException($"Line {rowLine}: a field exceeds 5,000 characters.");
            if (issues.Count > 100) break;
        }
        if (quoted) issues.Add(new(rowLine, "CSV", "The quoted field is missing its closing quote.", true));
        if (field.Length > 0 || fields.Count > 0 || closedQuote) EndRow();
        if (records.Count == 0) return new(separator, [], [], [new(1, "CSV", "The file has no header row.", true)]);
        var headers = records[0].Cells.Select((value, index) => string.IsNullOrWhiteSpace(value) ? $"Column {index + 1}" : value.Trim()).ToArray();
        foreach (var row in records.Skip(1))
            if (row.Cells.Count != headers.Length)
                issues.Add(new(row.Line, "CSV", $"Expected {headers.Length} fields but found {row.Cells.Count}.", true));
        return new(separator, headers, records.Skip(1).ToArray(), issues);
    }

    /// <summary>Suggests conventional headers; the user can revise every suggestion before import.</summary>
    public static CatalogStudioColumnMap SuggestMapping(IReadOnlyList<string> headers)
    {
        int Find(params string[] aliases) => Enumerable.Range(0, headers.Count)
            .FirstOrDefault(index => aliases.Contains(new string(headers[index].Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant()), -1);
        return new()
        {
            Sku = Find("sku", "code", "productcode", "stockcode"),
            Name = Find("name", "title", "product", "productname"),
            Price = Find("price", "unitprice", "amount"),
            Description = Find("description", "details", "summary"),
            Category = Find("category", "collection", "group"),
            Stock = Find("stock", "quantity", "inventory"),
            Image = Find("image", "photo", "filename", "imagefilename", "imagefile")
        };
    }

    /// <summary>Maps a structurally valid table into editable product rows without discarding invalid prices or SKUs.</summary>
    public static List<CatalogStudioProduct> Import(CatalogStudioCsvTable table, CatalogStudioColumnMap map)
    {
        if (table.Issues.Any(issue => issue.IsError)) throw new FormatException("Repair the CSV structure before importing its rows.");
        if (table.Rows.Count == 0) throw new FormatException("The CSV contains a header but no product rows.");
        if (map.Sku < 0 || map.Name < 0 || map.Price < 0) throw new FormatException("Map SKU, product name and price before importing.");
        var columns = new[] { map.Sku, map.Name, map.Price, map.Description, map.Category, map.Stock, map.Image };
        if (columns.Any(index => index < -1 || index >= table.Headers.Count)) throw new FormatException("A mapped source column does not exist.");
        if (columns.Where(index => index >= 0).Distinct().Count() != columns.Count(index => index >= 0))
            throw new FormatException("Map each source column to no more than one catalog field.");
        return table.Rows.Select(row =>
        {
            string Cell(int index) => index < 0 ? "" : row.Cells[index].Trim();
            return new CatalogStudioProduct
            {
                SourceRow = row.Line, Sku = Cell(map.Sku), Name = Cell(map.Name), Price = Cell(map.Price),
                Description = Cell(map.Description), Category = Cell(map.Category), Stock = Cell(map.Stock), ImageHint = Cell(map.Image)
            };
        }).ToList();
    }
}
