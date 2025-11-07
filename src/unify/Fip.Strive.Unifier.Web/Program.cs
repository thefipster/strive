using Fip.Strive.Core.Web;
using Fip.Strive.Unifier.Application;
using Fip.Strive.Unifier.Web.Components;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddCoreServices(configuration);
builder.Services.AddApplicationServices(configuration);

var app = builder.Build();

app.UseCoreServices<App>();

app.Run();
