namespace Fip.Strive.AppHost.Systems;

public static class Strive
{
    public static void AddStriveProject(this IDistributedApplicationBuilder builder)
    {
        builder.AddProject<Projects.Fip_Strive_Apps_AdminWeb>("strive-web-admin");
    }
}
