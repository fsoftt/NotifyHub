using MongoDB.Driver;

namespace NotifyHub.Api.Infrastructure.Notifications;

public static class NotificationIndexes
{
    public static async Task EnsureCreatedAsync(IMongoDatabase database)
    {
        var collection = database.GetCollection<NotificationDocument>(NotificationDocument.CollectionName);

        // Matches List's real query shape: filter by Email, sort by CreatedAt descending.
        var emailCreatedAtIndex = new CreateIndexModel<NotificationDocument>(
            Builders<NotificationDocument>.IndexKeys
                .Ascending(x => x.Email)
                .Descending(x => x.CreatedAt),
            new CreateIndexOptions { Name = "email_createdAt" });

        await collection.Indexes.CreateOneAsync(emailCreatedAtIndex);
    }
}
