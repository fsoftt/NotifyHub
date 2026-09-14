var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "NotifyHub API");

app.Run();
