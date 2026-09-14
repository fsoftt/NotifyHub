using MongoDB.Driver;
using NotifyHub.Api.Infrastructure.Mongo;
using NotifyHub.Api.Infrastructure.Notifications;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MongoDbOptions>(
    builder.Configuration.GetSection(MongoDbOptions.SectionName));
builder.Services.AddSingleton<MongoContext>();

var app = builder.Build();

app.MapGet("/", () => "NotifyHub API");

app.MapPost("/notifications", async (CreateNotificationRequest request, MongoContext mongoContext) =>
{
    var collection = mongoContext.Database.GetCollection<NotificationDocument>(NotificationDocument.CollectionName);

    var document = new NotificationDocument
    {
        Email = request.Email,
        Type = request.Type,
        CreatedAt = DateTime.UtcNow
    };

    await collection.InsertOneAsync(document);

    return Results.Created($"/notifications/{document.Id}", document);
});

app.MapGet("/notifications/{id}", async (string id, MongoContext mongoContext) =>
{
    var collection = mongoContext.Database.GetCollection<NotificationDocument>(NotificationDocument.CollectionName);
    var filter = Builders<NotificationDocument>.Filter.Eq(x => x.Id, id);
    var cursor = await collection.FindAsync(filter);
    var document = await cursor.FirstOrDefaultAsync();

    return document is null ? Results.NotFound() : Results.Ok(document);
});

app.Run();

record CreateNotificationRequest(string Email, string Type);
