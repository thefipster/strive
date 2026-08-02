using Fip.Strive.Application.Features.Catalog.Models;
using Fip.Strive.Application.Features.Catalog.Services.Contracts;
using Microsoft.AspNetCore.Components;

namespace Fip.Strive.Web.Components.Pages;

public partial class PackagesPage
{
    [Inject]
    private ICatalogReader Catalog { get; set; } = default!;

    private IReadOnlyList<PackageRow>? _packages;

    protected override async Task OnInitializedAsync() =>
        _packages = await Catalog.GetPackagesAsync();

    private static double NewShare(PackageRow package) =>
        package.FileCount == 0 ? 0 : (double)package.NewEntryCount / package.FileCount;
}
