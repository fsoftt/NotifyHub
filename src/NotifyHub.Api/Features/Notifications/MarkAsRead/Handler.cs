using MongoDB.Driver;
using NotifyHub.Api.Infrastructure.Mongo;
using NotifyHub.Api.Infrastructure.Mongo.Documents;

namespace NotifyHub.Api.Features.Notifications.MarkAsRead
{
    public sealed class Handler
    {
        private readonly MongoContext context;

        public Handler(MongoContext context)
        {
            this.context = context;
        }

        public async Task<bool> HandleAsync(
            Command command,
            CancellationToken cancellationToken = default)
        {
            var collection = context.GetCollection<NotificationDocument>("notifications");
            var filter =
                Builders<NotificationDocument>
                    .Filter
                    .Eq(n => n.Id, command.Id);
            
            var update =
                Builders<NotificationDocument>
                    .Update
                    .Set(n => n.Read, true)
                    .Set(n => n.ReadAt, DateTimeOffset.UtcNow);

            var result = await collection.UpdateOneAsync(
                filter, 
                update, 
                cancellationToken: cancellationToken);

            return result.ModifiedCount > 0;
        }
    }
}
