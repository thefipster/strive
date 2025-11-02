using Fip.Strive.Core.Web;
using Fip.Strive.Portal.Web.Components;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddCoreServices(configuration);

var app = builder.Build();

app.UseCoreServices<App>();

app.Run();
