using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using NotifyHub.Api.Features.Notifications.Create;
using NotifyHub.Api.Features.Notifications.GetById;
using NotifyHub.Api.Features.Notifications.List;
using NotifyHub.Api.Features.Notifications.MarkAllAsRead;
using NotifyHub.Api.Features.Notifications.MarkAsRead;
using NotifyHub.Api.Infrastructure.Email;
using NotifyHub.Api.Infrastructure.Messaging;
using NotifyHub.Api.Infrastructure.Mongo;
using NotifyHub.Api.Infrastructure.Notifications;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MongoDbOptions>(
    builder.Configuration.GetSection(MongoDbOptions.SectionName));
builder.Services.AddSingleton<MongoContext>();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.Configure<ResendOptions>(
    builder.Configuration.GetSection(ResendOptions.SectionName));
builder.Services.AddHttpClient(ResendEmailSender.HttpClientName, client =>
{
    client.BaseAddress = new Uri("https://api.resend.com/");
    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", builder.Configuration["Email:ApiKey"]);
});
builder.Services.AddSingleton<IEmailSender, ResendEmailSender>();
builder.Services.AddSingleton<EmailNotificationProcessor>();

var rabbitMqOptions = builder.Configuration.GetSection(RabbitMqOptions.SectionName).Get<RabbitMqOptions>()
    ?? new RabbitMqOptions();
var rabbitMqConnection = await RabbitMqConnection.CreateAsync(rabbitMqOptions);
builder.Services.AddSingleton(rabbitMqConnection);

var rabbitMqPublisher = await RabbitMqPublisher.CreateAsync(rabbitMqConnection);
builder.Services.AddSingleton(rabbitMqPublisher);

builder.Services.AddSingleton<NotificationConsumer>();

builder.Services.AddHostedService<OutboxProcessor>();
builder.Services.AddHostedService<RabbitMqConsumerWorker>();

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
