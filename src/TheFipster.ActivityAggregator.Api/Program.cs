using TheFipster.ActivityAggregator.Api;
using TheFipster.ActivityAggregator.Storage.Lite;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Services.AddCorsPolicies();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddLiteDbStorage(config);
builder.Services.AddCustom(config);

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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
