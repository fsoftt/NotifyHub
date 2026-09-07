using MongoDB.Driver;
using NotifyHub.Api.Infrastructure.Mongo;
using NotifyHub.Api.Infrastructure.Mongo.Documents;

namespace NotifyHub.Api.Features.Notifications.GetById
{
    public sealed class Handler
    {
        private readonly MongoContext context;

        public Handler(MongoContext context)
        {
            this.context = context;
        }

        public async Task<Response> HandleAsync(
            Query query, 
            CancellationToken cancellationToken)
        {
            var collection = context.GetCollection<NotificationDocument>("notifications");

            var filter = Builders<NotificationDocument>
                .Filter.Eq(x => x.Id, query.Id);

            var projection = Builders<NotificationDocument>
                .Projection
                .Expression(x => new Response(
                    x.Id,
                    x.UserId,
                    x.Type,
                    x.Content.Title,
                    x.Content.Message,
                    x.Read,
                    x.ReadAt,
                    x.CreatedAt
                ));

            return await collection
                .Find(filter)
                .Project(projection)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
