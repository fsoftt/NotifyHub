using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace NotifyHub.Api.Infrastructure.Mongo;

public class MongoContext
{
    public MongoContext(IOptions<MongoDbOptions> options)
    {
        var mongoDbOptions = options.Value;
        Client = new MongoClient(mongoDbOptions.ConnectionString);
        Database = Client.GetDatabase(mongoDbOptions.DatabaseName);
    }

    public IMongoClient Client { get; }

    public IMongoDatabase Database { get; }
}
