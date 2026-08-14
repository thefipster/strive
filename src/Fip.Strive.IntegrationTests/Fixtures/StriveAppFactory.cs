using Fip.Strive.Application;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Fip.Strive.IntegrationTests.Fixtures;

/// <summary>
/// Boots the real web host against a throwaway database and data directory. Startup runs the
/// migrations, so a broken migration fails these tests rather than a deployment.
/// </summary>
public sealed class StriveAppFactory(string connectionString, string dataDirectory)
    : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting($"ConnectionStrings:{Registration.DatabaseName}", connectionString);
        builder.UseSetting("Storage:DataDirectory", dataDirectory);

        // These tests boot the host to exercise something else. A live runner polling in the
        // background would be noise at best and a race at worst.
        builder.UseSetting("Jobs:Enabled", "false");
    }
}
