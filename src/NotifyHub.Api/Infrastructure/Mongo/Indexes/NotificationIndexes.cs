using MongoDB.Driver;
using NotifyHub.Api.Infrastructure.Mongo.Documents;

namespace NotifyHub.Api.Infrastructure.Mongo.Indexes
{
    public static class NotificationIndexes
    {
        public static async Task CreateAsync(
            IMongoCollection<NotificationDocument> collection,
            CancellationToken cancellationToken = default)
        {
            var index = new CreateIndexModel<NotificationDocument>(
                Builders<NotificationDocument>.IndexKeys
                    .Ascending(x => x.UserId)
                    .Descending(x => x.CreatedAt)
                    .Descending(x => x.Id),
                new CreateIndexOptions
                {
                    Name = "IX_Notifications_UserId_CreatedAt_Id",
                });

            await collection.Indexes.CreateOneAsync(
                index, 
                cancellationToken: cancellationToken);

        }
    }
}
