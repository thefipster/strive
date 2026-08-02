using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Fip.Strive.Application.Infrastructure;

/// <summary>
/// Used only by <c>dotnet ef</c>. Adding a migration never opens a connection, so the placeholder
/// only has to be a parseable Npgsql string; point <c>STRIVE_DESIGN_TIME_CONNECTION</c> at a real
/// database for commands that do connect (<c>database update</c>, <c>dbcontext script</c>).
/// </summary>
public class StriveContextFactory : IDesignTimeDbContextFactory<StriveContext>
{
    private const string ConnectionVariable = "STRIVE_DESIGN_TIME_CONNECTION";

    private const string Placeholder =
        "Host=localhost;Port=5432;Database=strive;Username=postgres;Password=postgres";

    public StriveContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable(ConnectionVariable) ?? Placeholder;

        var options = new DbContextOptionsBuilder<StriveContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new StriveContext(options);
    }
}
