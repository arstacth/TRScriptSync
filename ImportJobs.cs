using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using MySqlConnector;

namespace TRScriptSync
{
    public static class ImportJobs
    {
        static readonly Regex Ident = new Regex(@"^[A-Za-z0-9_]+$", RegexOptions.Compiled);
        static readonly Dictionary<string, string> NameAlias = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "groupidx", "groupitemdescnum" },
            { "memberidx", "memberitemdescnum" },
            { "active", "activetype" },
            { "compcount", "compcount" },
            { "eventtype", "competitioneventtype" },
        };
        static readonly HashSet<string> SkipDbs = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "information_schema", "mysql", "performance_schema", "sys", "phpmyadmin"
        };

        // script/tblluckybagresultitems_1.txt -> db.tblluckybagresultitems_1
        // The file stem is the table name. Create it in the selected database when it is not there yet.
        static readonly Dictionary<string, string> CreateIfMissing = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "tblluckybagresultitems_1",
                "CREATE TABLE IF NOT EXISTS `tblluckybagresultitems_1` ("
                + "`fdNum` int(11) NOT NULL,"
                + "`fdItemDescNum` int(11) NOT NULL,"
                + "`fdLuckyBagNum` int(11) NOT NULL,"
                + "`fdCount` int(11) NOT NULL,"
                + "KEY `fdItemDescNum` (`fdItemDescNum`,`fdLuckyBagNum`) USING BTREE"
                + ") ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci ROW_FORMAT=DYNAMIC" },
        };

        public static string Q(string name)
        {
            if (string.IsNullOrEmpty(name) || !Ident.IsMatch(name))
                throw new InvalidOperationException("Unsafe SQL name: " + name);
            return "`" + name + "`";
        }

        public static MySqlConnection Open(AppConfig cfg, bool includeDb)
        {
            var conn = new MySqlConnection(cfg.ConnStr(includeDb));
            conn.Open();
            foreach (string sql in new string[] {
                "SET NAMES utf8mb4",
                "SET FOREIGN_KEY_CHECKS=0",
                "SET UNIQUE_CHECKS=0",
                "SET SESSION sql_mode='NO_ENGINE_SUBSTITUTION'",
                "SET SESSION net_read_timeout=600",
                "SET SESSION net_write_timeout=600",
                "SET SESSION wait_timeout=600" })
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.ExecuteNonQuery();
                }
            }
            return conn;
        }

        public static void Ping(AppConfig cfg)
        {
            using (var conn = Open(cfg, false))
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT 1";
                cmd.ExecuteScalar();
            }
        }

        public static List<string> ListDatabases(AppConfig cfg)
        {
            var names = new List<string>();
            using (var conn = Open(cfg, false))
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SHOW DATABASES";
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        string n = r.GetString(0);
                        if (!SkipDbs.Contains(n))
                            names.Add(n);
                    }
                }
            }
            names.Sort(StringComparer.OrdinalIgnoreCase);
            return names;
        }

        public static Dictionary<string, string> MatchingTables(AppConfig cfg, IEnumerable<string> stems)
        {
            var names = new List<string>();
            foreach (string s in stems)
            {
                if (!string.IsNullOrEmpty(s) && Ident.IsMatch(s))
                    names.Add(s.ToLowerInvariant());
            }
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (names.Count == 0)
                return map;
            var quoted = new List<string>();
            foreach (string n in names)
                quoted.Add("'" + n.Replace("'", "''") + "'");
            using (var conn = Open(cfg, true))
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText =
                    "SELECT TABLE_NAME FROM information_schema.TABLES WHERE TABLE_SCHEMA=@db AND LOWER(TABLE_NAME) IN ("
                    + string.Join(",", quoted) + ")";
                cmd.Parameters.AddWithValue("@db", cfg.Database);
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        string table = r.GetString(0);
                        map[table] = table;
                    }
                }
            }
            return map;
        }

        public static List<ReadyFile> Scan(AppConfig cfg, Action<string> log)
        {
            if (string.IsNullOrWhiteSpace(cfg.Folder) || !Directory.Exists(cfg.Folder))
                throw new InvalidOperationException("Folder not found.");
            Ping(cfg);
            var files = new List<string>(Directory.GetFiles(cfg.Folder, "*.txt", SearchOption.AllDirectories));
            files.Sort(StringComparer.OrdinalIgnoreCase);
            var stems = new List<string>();
            foreach (string f in files)
                stems.Add(Path.GetFileNameWithoutExtension(f));
            Dictionary<string, string> found = MatchingTables(cfg, stems);
            var ready = new List<ReadyFile>();
            int skipped = 0;
            foreach (string path in files)
            {
                string stem = Path.GetFileNameWithoutExtension(path);
                string table;
                found.TryGetValue(stem, out table);
                if (string.IsNullOrEmpty(table) && CreateIfMissing.ContainsKey(stem))
                    table = stem;
                string rel = path.Substring(cfg.Folder.TrimEnd('\\', '/').Length).TrimStart('\\', '/');
                if (!string.IsNullOrEmpty(table) && ScriptFile.HasData(path))
                {
                    ready.Add(new ReadyFile { Path = path, Table = table, Rel = rel });
                    if (log != null)
                        log("ready  " + rel + "  ->  " + table);
                }
                else
                {
                    skipped++;
                    if (log != null)
                    {
                        if (string.IsNullOrEmpty(table))
                            log("skip   " + rel + "  (no table)");
                        else
                            log("skip   " + rel + "  (empty)");
                    }
                }
            }
            if (log != null)
                log(ready.Count + " ready, " + skipped + " skipped");
            return ready;
        }

        public static string ImportFolder(AppConfig cfg, Action<int, int, string> progress)
        {
            return ImportFolder(cfg, progress, null);
        }

        public static string ImportFolder(AppConfig cfg, Action<int, int, string> progress, IEnumerable<string> onlyTables)
        {
            List<ReadyFile> ready = Scan(cfg, null);
            if (onlyTables != null)
            {
                var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (string t in onlyTables)
                {
                    string n = (t ?? "").Trim();
                    if (n.Length > 0)
                        set.Add(n);
                }
                if (set.Count > 0)
                    ready = ready.FindAll(r => set.Contains(r.Table));
            }
            return ImportReady(cfg, ready, progress);
        }

        // Tables that feed usp_makeRealItemListTable (PopularItem / CPKRef / RealList / Pet / Farm caches).
        static readonly HashSet<string> ItemLookupTriggerTables = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "tblavataritemdesc",
            "tblavataritemdescex",
            "tblavataritemdescattr",
            "tblavataritemdesclevellimit",
            "tblavataritemconvertable",
            "tblavataritemsetdesc",
            "tblavataritemsetattr",
            "tblfarmitem",
            "tblfarmpetattr",
            "tblgamefarmitemattr",
        };

        public static string ImportReady(AppConfig cfg, List<ReadyFile> ready, Action<int, int, string> progress)
        {
            if (ready == null || ready.Count == 0)
                return "No matching TXT files.";
            int ok = 0;
            int fail = 0;
            var errors = new List<string>();
            var imported = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < ready.Count; i++)
            {
                ReadyFile item = ready[i];
                if (progress != null)
                    progress(i, ready.Count, item.Rel);
                try
                {
                    ImportOne(cfg, item);
                    ok++;
                    imported.Add(item.Table);
                    if (progress != null)
                        progress(i + 1, ready.Count, "imported " + item.Table);
                }
                catch (Exception ex)
                {
                    fail++;
                    string err = item.Table + ": " + ex.Message;
                    errors.Add(err);
                    if (progress != null)
                        progress(i + 1, ready.Count, "error " + err);
                }
            }
            string msg = "Imported " + ok + "/" + ready.Count + ". Failed " + fail + ".";
            if (errors.Count > 0)
                msg += " " + string.Join(" | ", errors.ToArray());

            if (cfg.RebuildItemLookups && NeedsItemLookupRebuild(imported))
            {
                if (progress != null)
                    progress(ready.Count, ready.Count, "rebuilding item lookups (usp_makeRealItemListTable)...");
                try
                {
                    RebuildItemLookups(cfg);
                    msg += " Item lookups rebuilt.";
                    if (progress != null)
                        progress(ready.Count, ready.Count, "item lookups rebuilt");
                }
                catch (Exception ex)
                {
                    msg += " Item lookup rebuild failed: " + ex.Message;
                    if (progress != null)
                        progress(ready.Count, ready.Count, "item lookup rebuild failed: " + ex.Message);
                }
            }
            return msg;
        }

        static bool NeedsItemLookupRebuild(HashSet<string> importedTables)
        {
            foreach (string t in importedTables)
            {
                if (ItemLookupTriggerTables.Contains(t))
                    return true;
            }
            return false;
        }

        public static void RebuildItemLookups(AppConfig cfg)
        {
            using (var conn = Open(cfg, true))
            {
                try
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "CALL usp_makeRealItemListTable()";
                        cmd.CommandTimeout = 0;
                        cmd.ExecuteNonQuery();
                    }
                    return;
                }
                catch (Exception)
                {
                    // SP may be out of date vs table columns; fall back to the critical lookups.
                }
                RebuildItemLookupsFallback(conn);
            }
        }

        /// <summary>
        /// Minimal rebuild matching usp_makeRealItemListTable: PopularItem + CPKRef + RealList seed.
        /// Used when CALL usp_makeRealItemListTable fails (e.g. column-count drift).
        /// </summary>
        static void RebuildItemLookupsFallback(MySqlConnection conn)
        {
            Exec(conn, "TRUNCATE TABLE tblAvatarItemRealList");
            Exec(conn,
                "INSERT INTO tblAvatarItemRealList (fdItemDescNum, fdRealItemDescNum) "
                + "SELECT fdItemDescNum, fdRealItemDescNum FROM ("
                + " SELECT DISTINCT fdItemNum AS fdItemDescNum, fdItemNum AS fdRealItemDescNum "
                + " FROM tblAvatarItemDesc WHERE fdType = 1 "
                + " UNION ALL "
                + " SELECT DISTINCT T1.fdItemNum AS fdItemDescNum, T2.fdSupplyItemDescNum AS fdRealItemDescNum "
                + " FROM tblAvatarItemDesc T1 "
                + " JOIN tblAvatarItemDescEx T2 ON T1.fdItemNum = T2.fdVirtualItemDescNum "
                + " WHERE T1.fdType IN (3, 4)"
                + ") A");

            Exec(conn, "TRUNCATE TABLE EssenPopularItem");
            Exec(conn,
                "INSERT INTO EssenPopularItem (fdItemNum, fdCharacter, fdPosition, fdItemKind, fdNotDeleteWhenExpired) "
                + "SELECT fdItemNum, fdCharacter, fdPosition, fdItemKind, fdNotDeleteWhenExpired "
                + "FROM tblAvatarItemDesc WHERE "
                + "((fdCanStack = 1 OR fdOnOfftype > 0 OR fdPosition = 352 OR fdPosition = 820 OR fdType = 2 "
                + "OR (fdType = 1 AND fdCanUse = 1 AND fdPosition NOT IN (50000))) "
                + "AND (fdPosition <> 200 AND fdPosition <> 201)) "
                + "OR (fdPosition = 0 AND fdCharacter <> 0)");

            Exec(conn, "TRUNCATE TABLE EssenAvatarItemCPKRef");
            Exec(conn,
                "INSERT INTO EssenAvatarItemCPKRef (fdItemNum, fdChar, fdPos, fdKind) "
                + "SELECT fdItemNum, fdCharacter, fdPosition, fdItemKind "
                + "FROM tblAvatarItemDesc WHERE fdType IN (1,2,4)");
        }

        public static string DiagnoseFolder(AppConfig cfg, Action<string> log)
        {
            List<ReadyFile> ready = Scan(cfg, null);
            if (ready == null || ready.Count == 0)
                return "No matching TXT files.";
            int ok = 0;
            int fail = 0;
            var errors = new List<string>();
            foreach (ReadyFile item in ready)
            {
                string shadow = "_diag_" + item.Table;
                if (shadow.Length > 64)
                    shadow = shadow.Substring(0, 64);
                try
                {
                    using (var conn = Open(cfg, true))
                    {
                        Exec(conn, "DROP TABLE IF EXISTS " + Q(shadow));
                        if (TableExists(conn, cfg.Database, item.Table))
                            Exec(conn, "CREATE TABLE " + Q(shadow) + " LIKE " + Q(item.Table));
                        else
                        {
                            string ddl = DdlNamed(item.Table, shadow);
                            if (ddl == null)
                                throw new InvalidOperationException("No table " + item.Table);
                            Exec(conn, ddl);
                        }
                    }
                    var copy = new ReadyFile { Path = item.Path, Table = shadow, Rel = item.Rel };
                    ImportOne(cfg, copy);
                    ok++;
                    if (log != null)
                        log("ok " + item.Table);
                }
                catch (Exception ex)
                {
                    fail++;
                    string err = item.Table + ": " + ex.Message;
                    errors.Add(err);
                    if (log != null)
                        log("error " + err);
                }
                finally
                {
                    try
                    {
                        using (var conn = Open(cfg, true))
                            Exec(conn, "DROP TABLE IF EXISTS " + Q(shadow));
                    }
                    catch { }
                }
            }
            string msg = "Diagnosed " + ok + "/" + ready.Count + ". Failed " + fail + ".";
            if (errors.Count > 0)
                msg += " " + string.Join(" | ", errors.ToArray());
            return msg;
        }

        static bool TableExists(MySqlConnection conn, string db, string table)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText =
                    "SELECT COUNT(*) FROM information_schema.TABLES WHERE TABLE_SCHEMA=@db AND TABLE_NAME=@t";
                cmd.Parameters.AddWithValue("@db", db);
                cmd.Parameters.AddWithValue("@t", table);
                object n = cmd.ExecuteScalar();
                return Convert.ToInt32(n) > 0;
            }
        }

        static string DdlNamed(string canonical, string actualName)
        {
            string ddl;
            if (!CreateIfMissing.TryGetValue(canonical, out ddl))
                return null;
            return ddl.Replace("`" + canonical + "`", "`" + actualName + "`");
        }

        static void EnsureMappedTable(MySqlConnection conn, string table)
        {
            string ddl = DdlNamed(table, table);
            if (ddl != null)
                Exec(conn, ddl);
        }

        static List<ColInfo> Columns(MySqlConnection conn, string db, string table)
        {
            var cols = new List<ColInfo>();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText =
                    "SELECT COLUMN_NAME, DATA_TYPE, COLUMN_TYPE, IS_NULLABLE, COLUMN_KEY, "
                    + "IFNULL(COLUMN_DEFAULT,''), EXTRA, IFNULL(CHARACTER_MAXIMUM_LENGTH,''), "
                    + "IFNULL(CHARACTER_SET_NAME,''), IFNULL(COLLATION_NAME,'') "
                    + "FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=@db AND TABLE_NAME=@t "
                    + "ORDER BY ORDINAL_POSITION";
                cmd.Parameters.AddWithValue("@db", db);
                cmd.Parameters.AddWithValue("@t", table);
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        string maxLen = r.GetString(7);
                        int? len = null;
                        int parsed;
                        if (int.TryParse(maxLen, out parsed))
                            len = parsed;
                        cols.Add(new ColInfo
                        {
                            Name = r.GetString(0),
                            DataType = r.GetString(1),
                            ColumnType = r.GetString(2),
                            Nullable = string.Equals(r.GetString(3), "YES", StringComparison.OrdinalIgnoreCase),
                            Key = r.GetString(4),
                            Default = r.GetString(5),
                            Extra = r.GetString(6),
                            MaxLen = len,
                            Charset = r.GetString(8),
                            Collation = r.GetString(9),
                        });
                    }
                }
            }
            return cols;
        }

        static void ImportOne(AppConfig cfg, ReadyFile item)
        {
            ScriptFile script = ScriptFile.Read(item.Path);
            if (script.Lines.Count == 0)
                return;

            using (var conn = Open(cfg, true))
            {
            EnsureMappedTable(conn, item.Table);
            List<ColInfo> tableCols = Columns(conn, cfg.Database, item.Table);
            if (tableCols.Count == 0)
                throw new InvalidOperationException("No columns for " + item.Table);

            string[] header = BestHeader(script, tableCols);
            if (header != null)
                script.Header = header;
            try { AddMissingFileColumns(conn, item.Table, header, tableCols, script.Lines); }
            catch { }
            tableCols = Columns(conn, cfg.Database, item.Table);

            List<ColInfo> mapped;
            List<string> extraTable;
            MapColumns(script, tableCols, out mapped, out extraTable);
            if (mapped.Count == 0)
                throw new InvalidOperationException("No file columns match " + item.Table);

            var pk = new List<string>();
            foreach (ColInfo c in tableCols)
            {
                if ((c.Key ?? "").IndexOf("PRI", StringComparison.OrdinalIgnoreCase) >= 0)
                    pk.Add(c.Name);
            }
            bool pkInFile = pk.Count > 0;
            foreach (string p in pk)
            {
                bool foundPk = false;
                foreach (ColInfo c in mapped)
                {
                    if (string.Equals(c.Name, p, StringComparison.OrdinalIgnoreCase))
                        foundPk = true;
                }
                if (!foundPk)
                    pkInFile = false;
            }
            if (pk.Count > 0 && !pkInFile)
            {
                try { Exec(conn, "ALTER TABLE " + Q(item.Table) + " DROP PRIMARY KEY"); }
                catch { }
                tableCols = Columns(conn, cfg.Database, item.Table);
                MapColumns(script, tableCols, out mapped, out extraTable);
                pk.Clear();
            }

            var kinds = new string[mapped.Count];
            var nulls = new bool[mapped.Count];
            for (int i = 0; i < mapped.Count; i++)
            {
                kinds[i] = ScriptFile.KindOf(mapped[i].DataType);
                nulls[i] = mapped[i].Nullable;
            }

            var rows = new List<string[]>();
            foreach (string line in script.Lines)
            {
                try
                {
                    string[] vals = ScriptFile.ParseRow(line, kinds, nulls);
                    if (vals.Length == mapped.Count)
                        rows.Add(vals);
                }
                catch
                {
                }
            }
            if (rows.Count == 0)
                throw new InvalidOperationException("No usable data rows in " + item.Rel);

            FillMissingRequired(tableCols, mapped, extraTable, rows);

            try { Widen(conn, item.Table, mapped, rows); } catch { }

            pk.Clear();
            foreach (ColInfo c in tableCols)
            {
                if ((c.Key ?? "").IndexOf("PRI", StringComparison.OrdinalIgnoreCase) >= 0)
                    pk.Add(c.Name);
            }
            bool keep = cfg.KeepExtraColumns && extraTable.Count > 0 && pk.Count > 0;
            pkInFile = pk.Count > 0;
            if (pkInFile)
            {
                foreach (string p in pk)
                {
                    bool foundPk = false;
                    foreach (ColInfo c in mapped)
                    {
                        if (c.Name == p)
                            foundPk = true;
                    }
                    if (!foundPk)
                        pkInFile = false;
                }
            }

            DataTable dt = ToTable(mapped, rows);
            dt = Dedupe(dt, pk);
            if (keep && pkInFile && rows.Count < 20000)
            {
                string stg = ("_imp_" + item.Table);
                if (stg.Length > 64)
                    stg = stg.Substring(0, 64);
                try
                {
                    Exec(conn, "DROP TABLE IF EXISTS " + Q(stg));
                    Exec(conn, "CREATE TABLE " + Q(stg) + " LIKE " + Q(item.Table));
                    Bulk(conn, stg, dt);
                    var assigns = new List<string>();
                    var colList = new List<string>();
                    foreach (ColInfo c in mapped)
                    {
                        colList.Add(Q(c.Name));
                        assigns.Add(Q(c.Name) + "=VALUES(" + Q(c.Name) + ")");
                    }
                    string cols = string.Join(",", colList.ToArray());
                    Exec(conn,
                        "INSERT INTO " + Q(item.Table) + " (" + cols + ") SELECT " + cols + " FROM " + Q(stg)
                        + " ON DUPLICATE KEY UPDATE " + string.Join(",", assigns.ToArray()));
                    if (cfg.MatchFile)
                    {
                        var ons = new List<string>();
                        foreach (string p in pk)
                            ons.Add("t." + Q(p) + "=s." + Q(p));
                        Exec(conn,
                            "DELETE t FROM " + Q(item.Table) + " t LEFT JOIN " + Q(stg) + " s ON "
                            + string.Join(" AND ", ons.ToArray()) + " WHERE s." + Q(pk[0]) + " IS NULL");
                    }
                    Exec(conn, "DROP TABLE IF EXISTS " + Q(stg));
                    return;
                }
                catch
                {
                    try
                    {
                        if (conn.State == ConnectionState.Open)
                            Exec(conn, "DROP TABLE IF EXISTS " + Q(stg));
                    }
                    catch { }
                }
            }

            if (conn.State != ConnectionState.Open)
            {
                using (var conn2 = Open(cfg, true))
                    ReplaceTable(conn2, item.Table, dt);
                return;
            }
            ReplaceTable(conn, item.Table, dt);
            }
        }

        static string NormName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "";
            string n = name.Trim().ToLowerInvariant();
            if (n.StartsWith("fd") && n.Length > 2)
                n = n.Substring(2);
            return n;
        }

        static bool NamesMatch(string headerName, string colName)
        {
            if (string.Equals(headerName, colName, StringComparison.OrdinalIgnoreCase))
                return true;
            string a = NormName(headerName);
            string b = NormName(colName);
            if (a.Length == 0 || b.Length == 0)
                return false;
            if (a == b)
                return true;
            string alias;
            if (NameAlias.TryGetValue(a, out alias) && alias == b)
                return true;
            if (NameAlias.TryGetValue(b, out alias) && alias == a)
                return true;
            return false;
        }

        static ColInfo ResolveCol(string headerName, List<ColInfo> tableCols, HashSet<string> used)
        {
            foreach (ColInfo c in tableCols)
            {
                if (used.Contains(c.Name))
                    continue;
                if (NamesMatch(headerName, c.Name))
                    return c;
            }
            return null;
        }

        static string[] BestHeader(ScriptFile script, List<ColInfo> tableCols)
        {
            var cands = new List<string[]>();
            if (script.HeaderCandidates != null)
                cands.AddRange(script.HeaderCandidates);
            if (cands.Count == 0 && script.Header != null && script.Header.Length > 0)
                cands.Add(script.Header);
            if (cands.Count == 0)
                return script.Header;
            string[] best = cands[0];
            int bestScore = -1;
            foreach (string[] cand in cands)
            {
                int score = 0;
                foreach (string name in cand)
                {
                    if (ResolveCol(name, tableCols, new HashSet<string>(StringComparer.OrdinalIgnoreCase)) != null)
                        score += 2;
                }
                if (score > bestScore)
                {
                    bestScore = score;
                    best = cand;
                }
            }
            return best;
        }

        static string InferSqlType(IEnumerable<string> samples)
        {
            bool any = false;
            bool allInt = true;
            bool allFloat = true;
            bool allBool = true;
            bool allDate = true;
            bool allDt = true;
            int maxLen = 1;
            foreach (string raw in samples)
            {
                if (string.IsNullOrWhiteSpace(raw))
                    continue;
                any = true;
                string s = raw.Trim().Trim('"');
                if (s.Length > maxLen)
                    maxLen = s.Length;
                string low = s.ToLowerInvariant();
                if (low != "true" && low != "false" && low != "0" && low != "1")
                    allBool = false;
                int i;
                if (!int.TryParse(s, out i))
                    allInt = false;
                double d;
                if (!double.TryParse(s, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out d))
                    allFloat = false;
                DateTime dt;
                if (s.Length == 10 && DateTime.TryParseExact(s, "yyyy-MM-dd",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out dt))
                    allDt = false;
                else
                    allDate = false;
                if (!(s.Length >= 19 && DateTime.TryParse(s, out dt)))
                    allDt = false;
            }
            if (!any)
                return "varchar(255) NULL";
            if (allBool)
                return "tinyint(1) NOT NULL DEFAULT 0";
            if (allInt)
                return "int(11) NOT NULL DEFAULT 0";
            if (allFloat)
                return "decimal(18,3) NOT NULL DEFAULT 0";
            if (allDate)
                return "date NULL";
            if (allDt)
                return "datetime NULL";
            int len = 64;
            while (len < maxLen)
                len *= 2;
            if (len > 16383)
                return "mediumtext NULL";
            return "varchar(" + len + ") NULL";
        }

        static void AddMissingFileColumns(MySqlConnection conn, string table, string[] header,
            List<ColInfo> tableCols, List<string> lines)
        {
            if (header == null || header.Length == 0)
                return;
            var adds = new List<string>();
            for (int i = 0; i < header.Length; i++)
            {
                if (ResolveCol(header[i], tableCols, new HashSet<string>(StringComparer.OrdinalIgnoreCase)) != null)
                    continue;
                if (!Ident.IsMatch(header[i]))
                    continue;
                var samples = new List<string>();
                int take = Math.Min(40, lines.Count);
                for (int r = 0; r < take; r++)
                {
                    string[] parts = lines[r].Split(',');
                    if (i < parts.Length)
                        samples.Add(parts[i].Trim().Trim('"'));
                }
                adds.Add("ADD COLUMN " + Q(header[i]) + " " + InferSqlType(samples));
            }
            if (adds.Count > 0)
                Exec(conn, "ALTER TABLE " + Q(table) + " " + string.Join(",", adds.ToArray()));
        }

        static void FillMissingRequired(List<ColInfo> tableCols, List<ColInfo> mapped,
            List<string> extraTable, List<string[]> rows)
        {
            var mappedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (ColInfo c in mapped)
                mappedNames.Add(c.Name);
            int seq = 1;
            foreach (ColInfo c in tableCols)
            {
                if (mappedNames.Contains(c.Name))
                    continue;
                string extra = (c.Extra ?? "").ToLowerInvariant();
                if (extra.IndexOf("auto_increment", StringComparison.OrdinalIgnoreCase) >= 0)
                    continue;
                if (extra.IndexOf("generated", StringComparison.OrdinalIgnoreCase) >= 0)
                    continue;
                if (c.Nullable)
                    continue;
                if (!string.IsNullOrEmpty(c.Default))
                    continue;
                string kind = ScriptFile.KindOf(c.DataType);
                bool uniqueFill = (c.Key ?? "").IndexOf("PRI", StringComparison.OrdinalIgnoreCase) >= 0
                    || string.Equals(c.Name, "fdNum", StringComparison.OrdinalIgnoreCase);
                mapped.Add(c);
                mappedNames.Add(c.Name);
                extraTable.RemoveAll(n => string.Equals(n, c.Name, StringComparison.OrdinalIgnoreCase));
                for (int r = 0; r < rows.Count; r++)
                {
                    string fill;
                    if (uniqueFill && kind == "int")
                        fill = (seq++).ToString();
                    else if (kind == "int" || kind == "float" || kind == "bool")
                        fill = "0";
                    else
                        fill = "";
                    string[] old = rows[r];
                    var next = new string[old.Length + 1];
                    Array.Copy(old, next, old.Length);
                    next[old.Length] = fill;
                    rows[r] = next;
                }
            }
        }

        static void MapColumns(ScriptFile script, List<ColInfo> tableCols, out List<ColInfo> mapped, out List<string> extraTable)
        {
            mapped = new List<ColInfo>();
            extraTable = new List<string>();
            if (script.Header != null && script.Header.Length > 0)
            {
                var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (string name in script.Header)
                {
                    ColInfo col = ResolveCol(name, tableCols, used);
                    if (col != null)
                    {
                        mapped.Add(col);
                        used.Add(col.Name);
                    }
                }
                foreach (ColInfo c in tableCols)
                {
                    if (!used.Contains(c.Name))
                        extraTable.Add(c.Name);
                }
                return;
            }
            if (script.Lines.Count == 0)
                return;
            var kinds = new string[tableCols.Count];
            var nulls = new bool[tableCols.Count];
            for (int i = 0; i < tableCols.Count; i++)
            {
                kinds[i] = ScriptFile.KindOf(tableCols[i].DataType);
                nulls[i] = tableCols[i].Nullable;
            }
            try
            {
                string[] parsed = ScriptFile.ParseRow(script.Lines[0], kinds, nulls);
                if (parsed.Length == tableCols.Count)
                    mapped.AddRange(tableCols);
            }
            catch
            {
            }
        }

        static DataTable Dedupe(DataTable dt, List<string> pk)
        {
            var pkCols = new List<string>();
            foreach (string p in pk)
            {
                if (dt.Columns.Contains(p))
                    pkCols.Add(p);
            }
            if (pkCols.Count == 0)
                return dt;
            var last = new Dictionary<string, DataRow>();
            foreach (DataRow row in dt.Rows)
            {
                var parts = new string[pkCols.Count];
                for (int i = 0; i < pkCols.Count; i++)
                    parts[i] = Convert.ToString(row[pkCols[i]]) ?? "";
                last[string.Join("\0", parts)] = row;
            }
            if (last.Count == dt.Rows.Count)
                return dt;
            DataTable copy = dt.Clone();
            foreach (DataRow row in last.Values)
                copy.ImportRow(row);
            return copy;
        }

        static DataTable ToTable(List<ColInfo> mapped, List<string[]> rows)
        {
            var dt = new DataTable();
            foreach (ColInfo c in mapped)
                dt.Columns.Add(c.Name, typeof(string));
            foreach (string[] row in rows)
            {
                object[] cells = new object[row.Length];
                for (int i = 0; i < row.Length; i++)
                    cells[i] = row[i] ?? (object)DBNull.Value;
                dt.Rows.Add(cells);
            }
            return dt;
        }

        static void ReplaceTable(MySqlConnection conn, string table, DataTable dt)
        {
            try { Exec(conn, "TRUNCATE TABLE " + Q(table)); }
            catch { Exec(conn, "DELETE FROM " + Q(table)); }
            Bulk(conn, table, dt);
        }

        static void Bulk(MySqlConnection conn, string table, DataTable dt)
        {
            var colNames = new List<string>();
            foreach (DataColumn c in dt.Columns)
                colNames.Add(Q(c.ColumnName));
            string prefix = "INSERT INTO " + Q(table) + " (" + string.Join(",", colNames.ToArray()) + ") VALUES ";
            int batch = dt.Columns.Count > 20 ? 40 : 80;
            for (int i = 0; i < dt.Rows.Count; i += batch)
            {
                int endRow = i + batch;
                if (endRow > dt.Rows.Count)
                    endRow = dt.Rows.Count;
                using (var cmd = conn.CreateCommand())
                {
                    var parts = new List<string>();
                    int p = 0;
                    for (int r = i; r < endRow; r++)
                    {
                        var ph = new List<string>();
                        for (int c = 0; c < dt.Columns.Count; c++)
                        {
                            string name = "@p" + p;
                            ph.Add(name);
                            object val = dt.Rows[r][c];
                            if (val == null || val is DBNull)
                                cmd.Parameters.AddWithValue(name, DBNull.Value);
                            else
                                cmd.Parameters.AddWithValue(name, val);
                            p++;
                        }
                        parts.Add("(" + string.Join(",", ph.ToArray()) + ")");
                    }
                    cmd.CommandText = prefix + string.Join(",", parts.ToArray());
                    cmd.CommandTimeout = 0;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        static void Widen(MySqlConnection conn, string table, List<ColInfo> mapped, List<string[]> rows)
        {
            var alters = new List<string>();
            for (int i = 0; i < mapped.Count; i++)
            {
                ColInfo col = mapped[i];
                if (col.MaxLen == null || ScriptFile.KindOf(col.DataType) != "str")
                    continue;
                int longest = 0;
                foreach (string[] row in rows)
                {
                    if (row[i] != null && row[i].Length > longest)
                        longest = row[i].Length;
                }
                if (longest <= col.MaxLen.Value)
                    continue;
                int newLen = 128;
                while (newLen < longest)
                    newLen *= 2;
                if (newLen > 16383)
                    newLen = 16383;
                string charset = string.IsNullOrEmpty(col.Charset) ? "utf8" : col.Charset;
                string collation = string.IsNullOrEmpty(col.Collation) ? "utf8_unicode_ci" : col.Collation;
                string nullSql = col.Nullable ? "NULL" : "NOT NULL";
                alters.Add("MODIFY " + Q(col.Name) + " varchar(" + newLen + ") CHARACTER SET " + charset
                    + " COLLATE " + collation + " " + nullSql);
            }
            if (alters.Count > 0)
                Exec(conn, "ALTER TABLE " + Q(table) + " " + string.Join(",", alters.ToArray()));
        }

        static void Exec(MySqlConnection conn, string sql)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.CommandTimeout = 0;
                cmd.ExecuteNonQuery();
            }
        }

        sealed class ColInfo
        {
            public string Name;
            public string DataType;
            public string ColumnType;
            public bool Nullable;
            public string Key;
            public string Default;
            public string Extra;
            public int? MaxLen;
            public string Charset;
            public string Collation;
        }
    }
}
