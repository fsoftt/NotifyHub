using System.Text.Json.Serialization;
using NotifyHub.Api.Features.Notifications.Create;
using NotifyHub.Api.Features.Notifications.GetById;
using NotifyHub.Api.Features.Notifications.List;
using NotifyHub.Api.Features.Notifications.MarkAllAsRead;
using NotifyHub.Api.Features.Notifications.MarkAsRead;
using NotifyHub.Api.Infrastructure.Messaging;
using NotifyHub.Api.Infrastructure.Mongo;
using NotifyHub.Api.Infrastructure.Notifications;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MongoDbOptions>(
    builder.Configuration.GetSection(MongoDbOptions.SectionName));
builder.Services.AddSingleton<MongoContext>();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var rabbitMqOptions = builder.Configuration.GetSection(RabbitMqOptions.SectionName).Get<RabbitMqOptions>()
    ?? new RabbitMqOptions();
var rabbitMqConnection = await RabbitMqConnection.CreateAsync(rabbitMqOptions);
builder.Services.AddSingleton(rabbitMqConnection);

var rabbitMqPublisher = await RabbitMqPublisher.CreateAsync(rabbitMqConnection);
builder.Services.AddSingleton(rabbitMqPublisher);

builder.Services.AddHostedService<OutboxProcessor>();

var app = builder.Build();

await NotificationIndexes.EnsureCreatedAsync(app.Services.GetRequiredService<MongoContext>().Database);

await using (var setupChannel = await rabbitMqConnection.CreateChannelAsync())
{
    await NotificationsTopology.DeclareAsync(setupChannel);
}

app.MapGet("/", () => "NotifyHub API");

app.MapCreateNotification();
app.MapGetNotificationById();
app.MapListNotifications();
app.MapMarkNotificationAsRead();
app.MapMarkAllNotificationsAsRead();

app.Run();
