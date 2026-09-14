using NotifyHub.Api.Infrastructure.Mongo;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MongoDbOptions>(
    builder.Configuration.GetSection(MongoDbOptions.SectionName));
builder.Services.AddSingleton<MongoContext>();

var app = builder.Build();

app.MapGet("/", () => "NotifyHub API");

app.Run();
