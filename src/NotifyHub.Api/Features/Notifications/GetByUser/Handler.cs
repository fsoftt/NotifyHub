using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using NotifyHub.Api.Features.Notifications.GetById;
using NotifyHub.Api.Infrastructure.Mongo;
using NotifyHub.Api.Infrastructure.Mongo.Documents;

namespace NotifyHub.Api.Features.Notifications.GetByUser
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

            var filter =
                Builders<NotificationDocument>
                    .Filter
                    .Eq(n => n.UserId, query.UserId);

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
                .SortByDescending(n => n.CreatedAt)
                .Project(projection)
                .ToListAsync(cancellationToken);
        }
    }
}
