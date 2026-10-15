using System;
using System.IO;
using System.Windows.Forms;

namespace TRScriptSync
{
    internal static class Program
    {
        [STAThread]
        static int Main(string[] args)
        {
            if (args != null && args.Length >= 1 &&
                (args[0] == "--import" || args[0] == "--diagnose" || args[0] == "--import-only" || args[0] == "--rebuild-items"))
            {
                string logPath = Path.Combine(AppConfig.ExeDir(),
                    args[0] == "--diagnose" ? "diagnose.log" :
                    args[0] == "--rebuild-items" ? "rebuild.log" : "import.log");
                try
                {
                    File.WriteAllText(logPath, "");
                    AppConfig cfg = AppConfig.Load();
                    if (args[0] == "--rebuild-items")
                    {
                        ImportJobs.RebuildItemLookups(cfg);
                        string done = "Item lookups rebuilt (usp_makeRealItemListTable).";
                        Console.WriteLine(done);
                        File.AppendAllText(logPath, done + Environment.NewLine);
                        return 0;
                    }
                    if (args[0] == "--import-only")
                    {
                        if (args.Length < 2)
                            throw new InvalidOperationException("Usage: --import-only table1,table2 [folder]");
                        if (args.Length >= 3)
                            cfg.Folder = args[2];
                        string msgOnly = ImportJobs.ImportFolder(cfg, null, args[1].Split(','));
                        Console.WriteLine(msgOnly);
                        File.AppendAllText(logPath, msgOnly + Environment.NewLine);
                        return 0;
                    }
                    if (args.Length >= 2)
                        cfg.Folder = args[1];
                    string msg = args[0] == "--diagnose"
                        ? ImportJobs.DiagnoseFolder(cfg, line =>
                        {
                            Console.WriteLine(line);
                            File.AppendAllText(logPath, line + Environment.NewLine);
                        })
                        : ImportJobs.ImportFolder(cfg, null);
                    Console.WriteLine(msg);
                    File.AppendAllText(logPath, msg + Environment.NewLine);
                    return 0;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.Message);
                    File.AppendAllText(logPath, ex.ToString() + Environment.NewLine);
                    return 1;
                }
            }

            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
            return 0;
        }
    }
}
