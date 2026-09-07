using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace NotifyHub.Api.Infrastructure.Mongo
{
    public sealed class MongoContext
    {
        private readonly IMongoDatabase database;

        public MongoContext(IOptions<MongoOptions> options)
        {
            var mongoOptions = options.Value;
            var client = new MongoClient(mongoOptions.ConnectionString);
            database = client.GetDatabase(mongoOptions.DatabaseName);
        }

        public IMongoCollection<T> GetCollection<T>(string collectionName)
        {
            return database.GetCollection<T>(collectionName);
        }
    }
}
