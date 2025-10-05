using Serilog;
using TheFipster.ActivityAggregator.Api;
using TheFipster.ActivityAggregator.Api.Components;
using TheFipster.ActivityAggregator.Storage.Lite;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSerilog(c => c.ReadFrom.Configuration(builder.Configuration));
builder.Services.AddMetrics(builder.Configuration, builder.Environment);
builder.Services.AddCorsPolicies();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddLiteDbStorage(builder.Configuration);
builder.Services.AddCustom(builder.Configuration);

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseCors("AllowAll");
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseCors("AllowOne");
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
