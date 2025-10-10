using TheFipster.ActivityAggregator.Api.Setup;
using TheFipster.ActivityAggregator.Api.Setup.Application;
using TheFipster.ActivityAggregator.Api.Setup.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var environment = builder.Environment;

builder.Services.AddMonitoring(configuration, environment);
builder.Services.AddCorsPolicies();
builder.Services.AddExceptionHandler();
builder.Services.AddEndpoints();
builder.Services.AddApplicationServices(configuration);

var app = builder.Build();

app.UseCorsPolicy();
app.UseScalar();
app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseEndpoints();
app.Run();
