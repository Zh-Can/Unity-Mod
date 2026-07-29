using System.Collections.Generic;
using System.IO;
using System.Text;
using MelonLoader.Utils;

namespace LunHuiShop;

public static class ShopDataSaver
{
    private const string FileName = "LunhuiShop.cfg";

    private static string FilePath
    {
        get
        {
            var dir = MelonEnvironment.UserDataDirectory;
            if (string.IsNullOrEmpty(dir)) return FileName;
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            return Path.Combine(dir, FileName);
        }
    }

    public static bool Exists => File.Exists(FilePath);

    public static void Save(List<ShopItem> items)
    {
        if (items == null) return;

        var sb = new StringBuilder();
        sb.AppendLine("Id,Name,ItemLevel,Type,SortType,Price,Fame,IconName");

        foreach (var item in items)
        {
            sb.Append(CsvEscape(item.Id.ToString())).Append(',');
            sb.Append(CsvEscape(item.Name)).Append(',');
            sb.Append(CsvEscape(item.ItemLevel)).Append(',');
            sb.Append(CsvEscape(item.Type)).Append(',');
            sb.Append(CsvEscape(item.SortType)).Append(',');
            sb.Append(CsvEscape(item.Price.ToString())).Append(',');
            sb.Append(CsvEscape(item.Fame.ToString(System.Globalization.CultureInfo.InvariantCulture))).Append(',');
            sb.AppendLine(CsvEscape(item.IconName ?? ""));
        }

        File.WriteAllText(FilePath, sb.ToString(), Encoding.UTF8);
    }

    public static List<ShopItem> Load()
    {
        var path = FilePath;
        var list = new List<ShopItem>();
        if (!File.Exists(path)) return list;

        var lines = File.ReadAllLines(path, Encoding.UTF8);
        if (lines.Length <= 1) return list;

        list.Capacity = lines.Length - 1;
        for (var i = 1; i < lines.Length; i++)
        {
            var fields = ParseCsvLine(lines[i]);
            if (fields.Length < 7) continue;

            list.Add(new ShopItem
            {
                Id = int.Parse(fields[0]),
                Name = fields[1],
                ItemLevel = fields[2],
                Type = fields[3],
                SortType = fields[4],
                Price = int.Parse(fields[5]),
                Fame = float.Parse(fields[6], System.Globalization.CultureInfo.InvariantCulture),
                IconName = fields.Length > 7 ? fields[7] : null
            });
        }

        return list;
    }

    private static string CsvEscape(string value)
    {
        if (string.IsNullOrEmpty(value)) return "\"\"";
        if (value.Contains(',') || value.Contains('\"') || value.Contains('\n') || value.Contains('\r'))
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        return value;
    }

    private static string[] ParseCsvLine(string line)
    {
        var result = new List<string>();
        var sb = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];
            if (c == '\"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '\"')
                {
                    sb.Append('\"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(sb.ToString());
                sb.Clear();
            }
            else
            {
                sb.Append(c);
            }
        }

        result.Add(sb.ToString());
        return result.ToArray();
    }
}
