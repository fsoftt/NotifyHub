using NotifyHub.Api.Features.Notifications.Create;
using NotifyHub.Api.Features.Notifications.GetById;
using NotifyHub.Api.Features.Notifications.GetByUser;
using NotifyHub.Api.Features.Notifications.GetSummary;
using NotifyHub.Api.Features.Notifications.MarkAllAsRead;
using NotifyHub.Api.Features.Notifications.MarkAsRead;
using NotifyHub.Api.Infrastructure.Messaging;
using NotifyHub.Api.Infrastructure.Mongo;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services
    .AddOptions<MongoOptions>()
    .BindConfiguration(MongoOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services
    .AddOptions<RabbitMqOptions>()
    .BindConfiguration(RabbitMqOptions.SectionName)
    .ValidateOnStart();

builder.Services.AddScoped<MongoInitializer>();
builder.Services.AddScoped<RabbitMqPublisher>();
builder.Services.AddScoped<NotifyHub.Api.Features.Notifications.Create.Handler>();
builder.Services.AddScoped<NotifyHub.Api.Features.Notifications.GetById.Handler>();
builder.Services.AddScoped<NotifyHub.Api.Features.Notifications.GetByUser.Handler>();
builder.Services.AddScoped<NotifyHub.Api.Features.Notifications.GetSummary.Handler>();
builder.Services.AddScoped<NotifyHub.Api.Features.Notifications.MarkAsRead.Handler>();
builder.Services.AddScoped<NotifyHub.Api.Features.Notifications.MarkAllAsRead.Handler>();
builder.Services.AddSingleton<MongoContext>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var mongoInitializer = services.GetRequiredService<MongoInitializer>();
    await mongoInitializer.InitializeAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapCreateNotification();
app.MapGetNotificationsByUser();
app.MapGetNotificationById();
app.MapMarkNotificationAsRead();
app.MapMarkAllNotificationsAsRead();
app.MapGetNotificationsSummary();

app.Run();
