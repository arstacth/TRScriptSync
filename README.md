# TRScriptSync

TalesRunner Script Sync. It reads TalesRunner script `.txt` files and loads them into matching MySQL tables.

## Requirements

- Windows
- MySQL or MariaDB
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) to build

## Build

From this folder:

```bat
build.bat
```

Outputs:

- `bin\Debug\net8.0-windows\TRScriptSync.exe`
- `bin\publish\TRScriptSync.exe` (single file, no separate .NET install)

## Use the window

1. Choose the **Script folder**. It can contain subfolders. Only `*.txt` files are read.
2. Fill in MySQL **host / port / user / pass / database**.
3. Click **Scan** to see which files match a table.
4. Click **Import** to load those files.

Defaults are `127.0.0.1`, port `3306`, user `root`, an empty password, and database `tr_game_db`. The script folder starts empty. Nothing is saved to a settings file.

A file is imported when its name, without `.txt`, matches a table in the selected database. `itemdesc.txt` goes to `itemdesc`. Files with no matching table, and files with no data rows, are skipped.

`tblluckybagresultitems_1.txt` is created as `tblluckybagresultitems_1` in the selected database when that table is not there yet.

### Options

- **Keep extra DB columns.** Columns that are in the table but not in the file are left in place when the table has a primary key and that key is in the file. Otherwise the table is replaced with the file rows.
- **Make table match the file.** After the load, rows whose primary key is not in the file are deleted. This runs only together with **Keep extra DB columns**.
- **Rebuild item lookups after import.** After avatar item tables are imported, the program calls `usp_makeRealItemListTable()`.

Drag a folder or a file onto the window to set the script folder.

## Script file format

Blank lines are ignored. A line that starts with `_talesrunner_` is ignored. A `//` line with comma-separated names is the column header:

```text
//fdNum,fdItemDescNum,fdLuckyBagNum,fdCount
1,253,4677,1
2,256,4677,1
```

Header names can match table columns with or without an `fd` prefix.

## Command line

The commands below use the same MySQL defaults as the window (`127.0.0.1`, port `3306`, user `root`, empty password, database `tr_game_db`). Pass the script folder on the command line. Each run writes a log next to the exe: `import.log`, `diagnose.log`, or `rebuild.log`.

```bat
TRScriptSync.exe --import "C:\path\to\script"
TRScriptSync.exe --diagnose "C:\path\to\script"
TRScriptSync.exe --import-only tblavataritemdesc,tblfarmitem "C:\path\to\script"
TRScriptSync.exe --rebuild-items
```

- `--import` loads every matching file in the folder.
- `--diagnose` imports each file into a temporary copy of the table, then drops that copy. The real tables are not changed, except `tblluckybagresultitems_1` is created first when it does not exist.
- `--import-only` loads only the listed table names. The folder argument is optional for this command, but a folder is still required for the import to find files.
- `--rebuild-items` only runs `usp_makeRealItemListTable()`.
