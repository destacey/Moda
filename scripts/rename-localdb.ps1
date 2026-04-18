# scripts/rename-localdb.ps1
# Renames the LocalDB database [moda] to [wayd] in place to preserve your local
# data after the Moda -> Wayd rename.
#
# Usage: from the repository root:
#   pwsh ./scripts/rename-localdb.ps1
#
# Requires sqlcmd on PATH (ships with SQL Server tools / SSMS / Azure Data Studio).

$ErrorActionPreference = 'Stop'

$scriptPath = Join-Path $PSScriptRoot 'rename-localdb.sql'
if (-not (Test-Path $scriptPath)) {
    throw "SQL script not found: $scriptPath"
}

Write-Host 'Renaming LocalDB database [moda] -> [wayd]...'
& sqlcmd -S '(localdb)\mssqllocaldb' -E -b -i $scriptPath
if ($LASTEXITCODE -ne 0) {
    throw "sqlcmd failed with exit code $LASTEXITCODE"
}

Write-Host ''
Write-Host 'Done. If new migrations need to be applied, run:'
Write-Host '  dotnet ef database update --project Wayd.Infrastructure/src/Wayd.Infrastructure.Migrators.MSSQL --startup-project Wayd.Web/src/Wayd.Web.Api'
