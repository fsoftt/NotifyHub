using MongoDB.Driver;
using NotifyHub.Api.Infrastructure.Mongo;
using NotifyHub.Api.Infrastructure.Mongo.Documents;

namespace NotifyHub.Api.Features.Notifications.GetSummary
{
    public sealed class Handler
    {
        private readonly MongoContext context;

        public Handler(MongoContext context)
        {
            this.context = context;
        }

        public async Task<IReadOnlyCollection<Response>> HandleAsync(
            Query query,
            CancellationToken cancellationToken = default)
        {
            var collection = context.GetCollection<NotificationDocument>("notifications");

            var filter = Builders<NotificationDocument>
                .Filter
                .And(
                    Builders<NotificationDocument>
                        .Filter
                        .Eq(x => x.UserId, query.UserId),

                    Builders<NotificationDocument>
                        .Filter
                        .Eq(x => x.Read, false)
                );

            return await collection
                .Aggregate()
                .Match(filter)
                .Group(
                    x => x.Type, 
                    group => new Response(
                        Type: group.Key,
                        Count: group.Count()
                    ))
                .ToListAsync(cancellationToken);

        }
    }
}
