// Development-time orchestration only: it exists so `dotnet run` gives you a Postgres to work
// against. Production is still the single self-contained container from the roadmap — there the
// connection string and Storage__DataDirectory come from the environment, and nothing references
// Aspire.

// Must match the connection string name the web app resolves.
const string databaseName = "strive";

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder
    .AddPostgres("postgres")
    .WithDataVolume("strive-postgres-data")
    .WithPgAdmin(pgAdmin => pgAdmin.WithHostPort(8081));

var database = postgres.AddDatabase(databaseName);

builder
    .AddProject<Projects.Fip_Strive_Web>("strive-web")
    .WithReference(database)
    .WaitFor(database)
    .WithHttpHealthCheck("/health");

builder.Build().Run();
