using NotifyHub.Api.Infrastructure.Mongo;
using NotifyHub.Api.Infrastructure.Mongo.Documents;

namespace NotifyHub.Api.Features.Notifications.Create
{
    public sealed class Handler
    {
        private readonly MongoContext context;

        public Handler(MongoContext context)
        {
            this.context = context;
        }

        public async Task<string> Handle(
            Command command,
            CancellationToken cancellationToken)
        {
            var collection = context.GetCollection<NotificationDocument>("notifications");

            var document = new NotificationDocument
            {
                Id = Guid.NewGuid().ToString(),
                UserId = command.UserId,
                Type = command.Type,

                Content = new NotificationContentDocument
                {
                    Title = command.Title,
                    Message = command.Message
                },

                Channels = [],
                Read = false,
                CreatedAt = DateTimeOffset.UtcNow,
            };

            await collection.InsertOneAsync(
                document, 
                cancellationToken: cancellationToken);

            return document.Id;
        }
    }
}
