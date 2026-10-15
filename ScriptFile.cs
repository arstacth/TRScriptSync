using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace TRScriptSync
{
    public sealed class ScriptFile
    {
        static readonly Regex NumRe = new Regex(@"-?\d+(?:\.\d+)?(?:[eE][+-]?\d+)?", RegexOptions.Compiled);

        static readonly Regex HeaderIdent = new Regex(@"^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.Compiled);

        public string[] Header;
        public List<string[]> HeaderCandidates = new List<string[]>();
        public List<string> Lines = new List<string>();

        public static Encoding DetectEncoding(string path)
        {
            byte[] bom = new byte[8];
            int n;
            using (var fs = File.OpenRead(path))
                n = fs.Read(bom, 0, bom.Length);
            if (n >= 2 && bom[0] == 0xFF && bom[1] == 0xFE)
                return Encoding.Unicode;
            if (n >= 2 && bom[0] == 0xFE && bom[1] == 0xFF)
                return Encoding.BigEndianUnicode;
            if (n >= 4 && bom[0] != 0 && bom[1] == 0 && bom[3] == 0)
                return Encoding.Unicode;
            if (n >= 3 && bom[0] == 0xEF && bom[1] == 0xBB && bom[2] == 0xBF)
                return new UTF8Encoding(true);
            return new UTF8Encoding(false);
        }

        public static bool HasData(string path)
        {
            try
            {
                using (var reader = new StreamReader(path, DetectEncoding(path), true))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string s = line.Trim().Trim('\uFEFF');
                        if (s.Length == 0 || s.StartsWith("_talesrunner_") || s.StartsWith("//"))
                            continue;
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
            return false;
        }

        public static ScriptFile Read(string path)
        {
            var file = new ScriptFile();
            foreach (string raw in File.ReadLines(path, DetectEncoding(path)))
            {
                string s = raw.Trim().Trim('\uFEFF');
                if (s.Length == 0)
                    continue;
                if (s.StartsWith("_talesrunner_"))
                    continue;
                if (s.StartsWith("//"))
                {
                    string[] cols = ParseHeaderLine(s.Substring(2));
                    if (cols != null && cols.Length >= 2)
                    {
                        file.HeaderCandidates.Add(cols);
                        if (file.Header == null)
                            file.Header = cols;
                    }
                    continue;
                }
                file.Lines.Add(s);
            }
            return file;
        }

        public static string[] ParseHeaderLine(string body)
        {
            if (string.IsNullOrEmpty(body) || body.IndexOf(',') < 0)
                return null;
            var cols = new List<string>();
            int ident = 0;
            foreach (string part in body.Split(','))
            {
                string name = part.Trim().Trim('"').Trim();
                if (name.Length == 0)
                    continue;
                cols.Add(name);
                if (HeaderIdent.IsMatch(name))
                    ident++;
            }
            if (cols.Count < 2 || ident < cols.Count)
                return null;
            return cols.ToArray();
        }

        public static string KindOf(string dataType)
        {
            string dt = (dataType ?? "").ToLowerInvariant();
            if (dt == "varchar" || dt == "char" || dt == "text" || dt == "tinytext" || dt == "mediumtext"
                || dt == "longtext" || dt == "date" || dt == "datetime" || dt == "timestamp" || dt == "time"
                || dt == "blob" || dt == "tinyblob" || dt == "mediumblob" || dt == "longblob"
                || dt == "enum" || dt == "set" || dt == "json")
                return "str";
            if (dt == "float" || dt == "double" || dt == "decimal" || dt == "numeric" || dt == "real")
                return "float";
            if (dt == "tinyint" || dt == "smallint" || dt == "mediumint" || dt == "int" || dt == "integer"
                || dt == "bigint" || dt == "bit" || dt == "year")
                return "int";
            return "str";
        }

        static string DefaultFor(string kind, bool nullable)
        {
            if (nullable)
                return null;
            if (kind == "int" || kind == "float" || kind == "bool")
                return "0";
            return "";
        }

        static void SkipWs(string line, ref int pos)
        {
            while (pos < line.Length && (line[pos] == ' ' || line[pos] == '\t'))
                pos++;
        }

        static bool CanParse(string line, int pos, string kind)
        {
            while (pos < line.Length && (line[pos] == ' ' || line[pos] == '\t'))
                pos++;
            if (pos >= line.Length)
                return true;
            if (line[pos] == ',')
                return true;
            if (kind == "str")
                return true;
            string sl = line.Substring(pos, Math.Min(5, line.Length - pos)).ToLowerInvariant();
            if (sl.StartsWith("true") || sl.StartsWith("false"))
                return true;
            return NumRe.IsMatch(line, pos) && NumRe.Match(line, pos).Index == pos;
        }

        static string ParseOne(string line, ref int pos, string kind, bool nullable)
        {
            int n = line.Length;
            SkipWs(line, ref pos);
            if (pos >= n || line[pos] == ',')
                return DefaultFor(kind, nullable);

            if (kind == "str")
            {
                if (line[pos] == '"')
                {
                    pos++;
                    var buf = new StringBuilder();
                    while (pos < n)
                    {
                        char ch = line[pos];
                        if (ch == '"')
                        {
                            if (pos + 1 < n && line[pos + 1] == '"')
                            {
                                buf.Append('"');
                                pos += 2;
                                continue;
                            }
                            pos++;
                            break;
                        }
                        buf.Append(ch);
                        pos++;
                    }
                    return buf.ToString();
                }
                int start = pos;
                while (pos < n && line[pos] != ',')
                    pos++;
                return line.Substring(start, pos - start);
            }

            string sl = line.Substring(pos, Math.Min(5, n - pos)).ToLowerInvariant();
            if ((kind == "int" || kind == "float" || kind == "bool") && sl.StartsWith("true"))
            {
                pos += 4;
                return "1";
            }
            if ((kind == "int" || kind == "float" || kind == "bool") && sl.StartsWith("false"))
            {
                pos += 5;
                return "0";
            }

            Match m = NumRe.Match(line, pos);
            if (m.Success && m.Index == pos)
            {
                pos = m.Index + m.Length;
                return m.Value;
            }
            return DefaultFor(kind, nullable);
        }

        public static string[] ParseRow(string line, string[] kinds, bool[] nullables)
        {
            int pos = 0;
            var values = new string[kinds.Length];
            for (int i = 0; i < kinds.Length; i++)
            {
                if (i > 0)
                {
                    SkipWs(line, ref pos);
                    if (pos < line.Length && line[pos] == ',')
                        pos++;
                    SkipWs(line, ref pos);
                    int skips = 0;
                    while (skips < 3 && pos < line.Length && line[pos] == ',' && !CanParse(line, pos, kinds[i]))
                    {
                        pos++;
                        SkipWs(line, ref pos);
                        skips++;
                    }
                }
                string val = ParseOne(line, ref pos, kinds[i], nullables[i]);
                values[i] = Clean(val, kinds[i], nullables[i]);
            }
            return values;
        }

        public static string Clean(string raw, string kind, bool nullable)
        {
            if (raw == null)
                return nullable ? null : DefaultFor(kind, false);
            string s = raw.Trim();
            if (kind == "int" || kind == "float" || kind == "bool")
            {
                string low = s.ToLowerInvariant();
                if (low == "true")
                    return "1";
                if (low == "false")
                    return "0";
            }
            if (s.Length == 0)
                return DefaultFor(kind, nullable);
            return s.Replace("\\", "/").Replace("\t", " ").Replace("\r", " ").Replace("\n", " ");
        }
    }
}
