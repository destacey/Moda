-- scripts/rename-localdb.sql
-- Renames the LocalDB database moda to wayd in place, preserving data.
-- Run once after pulling the Moda -> Wayd rename PR.
--
-- Usage (from repo root):
--   sqlcmd -S "(localdb)\mssqllocaldb" -E -b -i scripts/rename-localdb.sql
-- Or via the PowerShell wrapper:
--   pwsh ./scripts/rename-localdb.ps1
--
-- If [moda] does not exist, this is a no-op. A fresh [wayd] database will be
-- created automatically by EF Core on first run of the API.

SET NOCOUNT ON;

USE master;
GO

-- Guard-and-rename in a single batch so RETURN actually short-circuits the rename.
IF DB_ID('moda') IS NULL
BEGIN
    PRINT 'Database [moda] does not exist. Nothing to rename.';
    PRINT 'If you need a fresh [wayd] database, start the API - EF Core will create it.';
END
ELSE IF DB_ID('wayd') IS NOT NULL
BEGIN
    RAISERROR('Database [wayd] already exists. Drop it or rename [moda] manually before re-running.', 16, 1);
END
ELSE
BEGIN
    PRINT 'Renaming [moda] -> [wayd]...';
    ALTER DATABASE moda SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    ALTER DATABASE moda MODIFY NAME = wayd;
    ALTER DATABASE wayd SET MULTI_USER;
    PRINT 'Database renamed: [moda] -> [wayd]';
END
GO
