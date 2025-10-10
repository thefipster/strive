using Scalar.AspNetCore;
using Serilog;
using TheFipster.ActivityAggregator.Api;
using TheFipster.ActivityAggregator.Api.Hubs;
using TheFipster.ActivityAggregator.Api.Middleware;
using TheFipster.ActivityAggregator.Api.Setup;
using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Storage.Lite;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var environment = builder.Environment;

builder.Services.AddLogging(configuration);
builder.Services.AddMetrics(configuration, environment);
builder.Services.AddCorsPolicies();
builder.Services.AddMiddleware();
builder.Services.AddProblemDetails();
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddLiteDbStorage(configuration);
builder.Services.AddApplicationServices(configuration);

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseCors(Const.Cors.AllowAll);
    app.MapOpenApi();
    app.MapScalarApiReference();
}
else
{
    app.UseCors(Const.Cors.AllowOne);
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHub<ImportHub>(Const.Hubs.Importer.Url);
app.Run();
