namespace Fip.Strive.Portal.Web.Models;

public class AppConfig
{
    public const string ConfigSectionName = "Portal:Apps";

    public required string Name { get; set; }
    public required string ShortName { get; set; }
    public required string ImageUrl { get; set; }
    public required string BaseUrl { get; set; }
    public required string Icon { get; set; }
}
