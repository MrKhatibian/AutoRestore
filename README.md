# AutoMirroring – SQL Server Auto Restore

A small Windows Forms (C#) tool that keeps a **mirror / replica database** up to date.
On startup it finds the **latest backup created today** in a source folder, copies it to the
SQL Server backup folder, and restores it over the target database.

## How it works

1. Builds today's date string (`yyyy_MM_dd`) in a culture-independent way.
2. Looks for `WebMarket*.bak` files in the **source folder**.
3. Picks the newest file whose name contains today's date.
4. Cleans old `WebMarket*.bak` files from the **destination folder** (SQL Server's `Backup` folder).
5. Copies the selected backup to the destination folder.
6. Restores the database:
   - `ALTER DATABASE ... SET SINGLE_USER WITH ROLLBACK IMMEDIATE`
   - `RESTORE DATABASE ... FROM DISK = @path WITH REPLACE`
   - `ALTER DATABASE ... SET MULTI_USER` (always executed, even if the restore fails)
7. Shows every step in the status label (`lblStatusInfo`).

## Requirements

- Windows + .NET Framework (Windows Forms)
- SQL Server (local instance, Windows Authentication)
- The user running the app must have:
  - read access to the source folder
  - write/delete access to SQL Server's `Backup` folder
  - `sysadmin` (or sufficient restore) permissions on the SQL Server instance
- The SQL Server service account must be able to read the destination folder

## Configuration

Edit the constants at the top of `AutoRestore.cs`:

| Constant | Description |
|---|---|
| `DatabaseName` | Name of the database to restore (also used as the `.bak` file name prefix) |
| `SourceFolderPath` | Folder where the backups are produced |
| `DestinationFolderPath` | SQL Server `Backup` folder |
| `ConnectionString` | Connection to the `master` database |
| `CommandTimeoutSeconds` | Timeout for restore commands (default: 600) |

Backup files are expected to look like `WebMarket_2025_01_31_14_30.bak` – i.e. the file name must
contain the date in `yyyy_MM_dd` format.

## Usage

1. Open the project in Visual Studio and adjust the settings.
2. Build and run – the restore starts automatically when the form loads.
3. For unattended mirroring, run the compiled `.exe` with **Windows Task Scheduler** and
   uncomment `Close();` at the end of the `Load` handler so the app exits when done.

## ⚠️ Warning

`WITH REPLACE` **overwrites the existing database** and all active connections are dropped.
Never point this tool at a production database. The tool also deletes other
`WebMarket*.bak` files in the destination folder.

## License

MIT (or your preferred license)
