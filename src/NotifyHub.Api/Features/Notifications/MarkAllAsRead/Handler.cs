using MongoDB.Driver;
using NotifyHub.Api.Infrastructure.Mongo;
using NotifyHub.Api.Infrastructure.Mongo.Documents;

namespace NotifyHub.Api.Features.Notifications.MarkAllAsRead
{
    public sealed class Handler
    {
        private readonly MongoContext context;

        public Handler(MongoContext context)
        {
            this.context = context;
        }

        public async Task<long> HandleAsync(
            Command command, 
            CancellationToken cancellationToken)
        {
            var collection = context.GetCollection<NotificationDocument>("notifications");

            var filter = Builders<NotificationDocument>
                .Filter
                .And(
                    Builders<NotificationDocument>
                        .Filter
                        .Eq(n => n.UserId, command.UserId),
                    Builders<NotificationDocument>
                        .Filter
                        .Eq(n => n.Read, false)
                );

            var update = Builders<NotificationDocument>
                .Update
                .Set(n => n.Read, true)
                .Set(n => n.ReadAt, DateTimeOffset.UtcNow);

            var result = await collection.UpdateManyAsync(
                filter, 
                update, 
                cancellationToken: cancellationToken);

            return result.ModifiedCount;
        }
    }
}
