using Microsoft.Extensions.Options;
using MongoDB.Driver;
using NotifyHub.Api.Infrastructure.Mongo;
using NotifyHub.Api.Infrastructure.Mongo.Documents;

namespace NotifyHub.Api.Infrastructure.Messaging
{
    public sealed class IdempotencyStore
    {
        private readonly MongoContext context;
        private readonly IdempotencyOptions options;

        public IdempotencyStore(MongoContext context, IOptions<IdempotencyOptions> options)
        {
            this.context = context;
            this.options = options.Value;
        }

        public async Task<bool> TryStartProcessingAsync(
            string messageId,
            CancellationToken cancellationToken)
        {
            var collection = context.GetCollection<ProcessedMessageDocument>("processed_messages");

            var now = DateTimeOffset.UtcNow;
            var leaseUntil = now.AddSeconds(options.LeaseSeconds);

            var filter =
                Builders<ProcessedMessageDocument>
                    .Filter.And(
                        Builders<ProcessedMessageDocument>
                            .Filter.Eq(x => x.Id, messageId),

                        Builders<ProcessedMessageDocument>
                            .Filter.Ne(x => x.Status, "Processed"),

                        Builders<ProcessedMessageDocument>
                            .Filter.Or(
                                Builders<ProcessedMessageDocument>
                                    .Filter.Ne(x => x.Status, "Processing"),

                                Builders<ProcessedMessageDocument>
                                    .Filter.Lte(x => x.LeaseUntil, now)
                            )
                    );

            var update =
                Builders<ProcessedMessageDocument>
                    .Update
                    .Set(x => x.Status, "Processing")
                    .Set(x => x.UpdatedAt, now)
                    .Set(x => x.LeaseUntil, leaseUntil);

            var existing = await collection.FindOneAndUpdateAsync(
                filter,
                update,
                new FindOneAndUpdateOptions<ProcessedMessageDocument>
                {
                    ReturnDocument = ReturnDocument.After
                }, cancellationToken);

            if (existing is not null)
            {
                return true;
            }

            try
            {
                var document = new ProcessedMessageDocument
                {
                    Id = messageId,
                    Status = "Processing",
                    UpdatedAt = now,
                    LeaseUntil = leaseUntil
                };

                await collection.InsertOneAsync(
                    document, 
                    cancellationToken: cancellationToken);

                return true;
            }
            catch (MongoWriteException ex)
            when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
            {
                return false;
            }
        }

        public async Task MarkProcessedAsync(
            string messageId,
            CancellationToken cancellationToken)
        {
            var collection = context.GetCollection<ProcessedMessageDocument>("processed_messages");

            var filter =
                Builders<ProcessedMessageDocument>
                    .Filter.Eq(x => x.Id, messageId);

            var update =
                Builders<ProcessedMessageDocument>
                    .Update
                    .Set(x => x.Status, "Processed")
                    .Set(x => x.UpdatedAt, DateTimeOffset.UtcNow)
                    .Unset(x => x.LeaseUntil);

            await collection.UpdateOneAsync(
                    filter, 
                    update, 
                    cancellationToken: cancellationToken);
        }

        public async Task<bool> RenewLeaseAsync(
            string messageId,
            CancellationToken cancellationToken)
        {
            var collection = context.GetCollection<ProcessedMessageDocument>("processed_messages");

            var now = DateTimeOffset.UtcNow;
            var leaseUntil = now.AddSeconds(options.LeaseSeconds);

            var filter =
                Builders<ProcessedMessageDocument>
                    .Filter.And(
                        Builders<ProcessedMessageDocument>
                            .Filter.Eq(x => x.Id, messageId),

                        Builders<ProcessedMessageDocument>
                            .Filter.Eq(x => x.Status, "Processing"),

                        Builders<ProcessedMessageDocument>
                            .Filter.Gt(x => x.LeaseUntil, now)
                    );

            var update =
                Builders<ProcessedMessageDocument>
                    .Update
                    .Set(x => x.UpdatedAt, now)
                    .Set(x => x.LeaseUntil, leaseUntil);

            var result = await collection.UpdateOneAsync(
                filter,
                update,
                cancellationToken: cancellationToken);
            
            return result.ModifiedCount == 1;
        }
    }
}
