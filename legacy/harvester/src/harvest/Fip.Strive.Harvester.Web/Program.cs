using Fip.Strive.Core.Application.Features.FileSystem;
using Fip.Strive.Core.Web;
using Fip.Strive.Harvester.Application.Features.Upload;
using Fip.Strive.Harvester.Web;
using Fip.Strive.Harvester.Web.Components;
using Fip.Strive.Ingestion.Application;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddCoreServices(configuration);
builder.Services.AddIndexing(configuration);
builder.Services.AddIngestionFeature();
builder.Services.AddUploadFeature(configuration);
builder.Services.AddFileSystemFeature();

var app = builder.Build();

app.UseCoreServices<App>();

app.Run();
