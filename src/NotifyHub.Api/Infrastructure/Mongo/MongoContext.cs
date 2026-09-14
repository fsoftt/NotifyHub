using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace NotifyHub.Api.Infrastructure.Mongo;

public class MongoContext
{
    public MongoContext(IOptions<MongoDbOptions> options)
    {
        var mongoDbOptions = options.Value;
        var mongoClient = new MongoClient(mongoDbOptions.ConnectionString);
        Database = mongoClient.GetDatabase(mongoDbOptions.DatabaseName);
    }

    public IMongoDatabase Database { get; }
}
