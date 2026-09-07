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

        public async Task<Response> HandleAsync(
            Query query,
            CancellationToken cancellationToken = default)
        {

            var collection = context.GetCollection<NotificationDocument>("notifications");

            var filter = Builders<NotificationDocument>
                .Filter
                .Eq(x => x.UserId, query.UserId);

            Cursor? cursor = null;
            if (!string.IsNullOrWhiteSpace(query.Cursor))
            {
                cursor = CursorEncoder.Decode(query.Cursor);

                var cursorFilter = Builders<NotificationDocument>
                    .Filter
                    .Or(
                        Builders<NotificationDocument>
                            .Filter
                            .Lt(x => 
                                x.CreatedAt,
                                cursor!.CreatedAt),
                        Builders<NotificationDocument>
                            .Filter
                            .And(
                                Builders<NotificationDocument>
                                    .Filter
                                    .Eq(
                                        x => x.CreatedAt,
                                        cursor.CreatedAt),

                                Builders<NotificationDocument>
                                    .Filter
                                    .Lt(
                                        x => x.Id,
                                        cursor.Id)
                            )
                    );

                filter = Builders<NotificationDocument>
                    .Filter
                    .And(filter, cursorFilter);
            }

            var projection = Builders<NotificationDocument>
                .Projection
                .Expression(x => new ResponseItem(
                    x.Id,
                    x.Type,
                    x.Content.Title,
                    x.Content.Message,
                    x.Read,
                    x.CreatedAt
                ));

            var items = await collection
                .Find(filter)
                .SortByDescending(n => n.CreatedAt)
                .ThenByDescending(n => n.Id)
                .Limit(query.PageSize + 1)
                .Project(projection)
                .ToListAsync(cancellationToken);

            var nextCursor = items.Count > query.PageSize
                ? new Cursor(
                    items.Last().CreatedAt,
                    items.Last().Id)
                : null;

            string cursorString = nextCursor is null ? string.Empty : CursorEncoder.Encode(nextCursor!)!;

            return new Response(
                Items: items.Take(query.PageSize).ToList(),
                NextCursor: cursorString);
        }
    }
}
