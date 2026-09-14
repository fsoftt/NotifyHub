using RabbitMQ.Client;

namespace NotifyHub.Api.Infrastructure.Messaging;

public sealed class RabbitMqConnection : IAsyncDisposable
{
    private readonly IConnection connection;

    private RabbitMqConnection(IConnection connection)
    {
        this.connection = connection;
    }

    // One long-lived connection for the application's lifetime (section 15) -
    // reconnect-on-failure logic is 5.14 (Connection recovery), not here.
    public static async Task<RabbitMqConnection> CreateAsync(
        RabbitMqOptions options,
        CancellationToken cancellationToken = default)
    {
        var factory = new ConnectionFactory
        {
            HostName = options.Host,
            Port = options.Port,
            UserName = options.UserName,
            Password = options.Password
        };

        var connection = await factory.CreateConnectionAsync(cancellationToken);

        return new RabbitMqConnection(connection);
    }

    public Task<IChannel> CreateChannelAsync(
        CreateChannelOptions? options = null,
        CancellationToken cancellationToken = default) =>
        connection.CreateChannelAsync(options, cancellationToken);

    public ValueTask DisposeAsync() => connection.DisposeAsync();
}
