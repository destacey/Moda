-- scripts/rename-localdb.sql
-- Renames the LocalDB database [Moda] to [Wayd] in place, preserving data.
-- Run once after pulling the Moda -> Wayd rename PR.
--
-- Usage (from repo root):
--   sqlcmd -S "(localdb)\mssqllocaldb" -E -b -i scripts/rename-localdb.sql
-- Or via the PowerShell wrapper:
--   pwsh ./scripts/rename-localdb.ps1
--
-- If [Moda] does not exist, this is a no-op. A fresh [Wayd] database will be
-- created automatically by EF Core on first run of the API.

SET NOCOUNT ON;

USE master;
GO

-- Guard-and-rename in a single batch so RETURN actually short-circuits the rename.
IF DB_ID('Moda') IS NULL
BEGIN
    PRINT 'Database [Moda] does not exist. Nothing to rename.';
    PRINT 'If you need a fresh [Wayd] database, start the API - EF Core will create it.';
END
ELSE IF DB_ID('Wayd') IS NOT NULL
BEGIN
    RAISERROR('Database [Wayd] already exists. Drop it or rename [Moda] manually before re-running.', 16, 1);
END
ELSE
BEGIN
    PRINT 'Renaming [Moda] -> [Wayd]...';
    ALTER DATABASE Moda SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    ALTER DATABASE Moda MODIFY NAME = Wayd;
    ALTER DATABASE Wayd SET MULTI_USER;
    PRINT 'Database renamed: [Moda] -> [Wayd]';
END
GO
