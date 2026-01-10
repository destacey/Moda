using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

// Option 1: External SQL Server (uses connection string from Moda.Web.Api/Configurations/database.json)
// Load connection string from Moda.Web.Api database.json configuration
var databaseConfigPath = Path.Combine(builder.AppHostDirectory, "..", "Moda.Web", "src", "Moda.Web.Api", "Configurations", "database.json");
var databaseConfig = new ConfigurationBuilder()
    .AddJsonFile(databaseConfigPath, optional: false)
    .Build();

var connectionString = databaseConfig["DatabaseSettings:ConnectionString"]
    ?? throw new InvalidOperationException("Connection string not found in database.json");

// Add the connection string as a parameter with the value from database.json
builder.Configuration["ConnectionStrings:ModaDb"] = connectionString;
var modaDb = builder.AddConnectionString("ModaDb");

// Option 2: SQL Server Container (for local development)
// Uncomment below to use a containerized SQL Server
// var sqlServer = builder.AddSqlServer("sql")
//     .WithDataVolume("moda-sqldata");
// var modaDb = sqlServer.AddDatabase("ModaDb");

var modaApi = builder.AddProject<Projects.Moda_Web_Api>("moda-api")
    .WithReference(modaDb)
    .WaitFor(modaDb);

builder.Build().Run();
