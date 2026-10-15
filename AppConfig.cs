using System;
using System.IO;

namespace TRScriptSync
{
    public sealed class AppConfig
    {
        public string Host { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 3306;
        public string User { get; set; } = "root";
        public string Password { get; set; } = "";
        public string Database { get; set; } = "tr_game_db";
        public string Folder { get; set; } = "";
        public bool MatchFile { get; set; } = true;
        public bool KeepExtraColumns { get; set; } = true;
        /// <summary>After importing avatar item tables, CALL usp_makeRealItemListTable() so inventory/CPK lookups refresh.</summary>
        public bool RebuildItemLookups { get; set; } = true;

        public static string ExeDir()
        {
            string path = Environment.ProcessPath;
            if (!string.IsNullOrEmpty(path))
            {
                string dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir))
                    return dir;
            }
            return AppContext.BaseDirectory;
        }

        public static AppConfig Load()
        {
            return new AppConfig();
        }

        public string ConnStr(bool includeDb)
        {
            string db = includeDb ? "Database=" + Database + ";" : "";
            return "Server=" + Host
                + ";Port=" + Port
                + ";User ID=" + User
                + ";Password=" + (Password ?? "")
                + ";" + db
                + "AllowLoadLocalInfile=true;CharacterSet=utf8mb4;ConnectionTimeout=8;DefaultCommandTimeout=0;";
        }
    }

    public sealed class ReadyFile
    {
        public string Path;
        public string Table;
        public string Rel;
    }
}
