using NotifyHub.Api.Infrastructure.Mongo.Documents;
using NotifyHub.Api.Infrastructure.Mongo.Indexes;

namespace NotifyHub.Api.Infrastructure.Mongo
{
    public sealed class MongoInitializer
    {
        private readonly MongoContext context;

        public MongoInitializer(MongoContext context)
        {
            this.context = context;
        }

        public async Task InitializeAsync(
            CancellationToken cancellationToken = default)
        {
            var notifications = context.GetCollection<NotificationDocument>("notifications");

            await NotificationIndexes.CreateAsync(
                notifications,
                cancellationToken);
        }
    }
}
