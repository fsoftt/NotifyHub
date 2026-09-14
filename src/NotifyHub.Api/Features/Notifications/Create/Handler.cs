using System.Text.RegularExpressions;
using MongoDB.Driver;
using NotifyHub.Api.Infrastructure.Mongo;
using NotifyHub.Api.Infrastructure.Notifications;

namespace NotifyHub.Api.Features.Notifications.Create;

internal static partial class Handler
{
    internal static Dictionary<string, string[]> Validate(Command command)
    {
        var errors = new Dictionary<string, List<string>>();

        void AddError(string field, string message)
        {
            if (!errors.TryGetValue(field, out var messages))
            {
                messages = [];
                errors[field] = messages;
            }

            messages.Add(message);
        }

        if (string.IsNullOrWhiteSpace(command.Email) || !EmailRegex().IsMatch(command.Email))
        {
            AddError(nameof(Command.Email), "A valid email address is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Type))
        {
            AddError(nameof(Command.Type), "Type is required.");
        }

        if (string.IsNullOrWhiteSpace(command.ContentTitle))
        {
            AddError(nameof(Command.ContentTitle), "Content title is required.");
        }

        if (string.IsNullOrWhiteSpace(command.ContentMessage))
        {
            AddError(nameof(Command.ContentMessage), "Content message is required.");
        }

        if (command.Channels.Count == 0)
        {
            AddError(nameof(Command.Channels), "At least one channel is required.");
        }
        else if (command.Channels.Distinct().Count() != command.Channels.Count)
        {
            // Section 7's model relies on channels[].type being unique per notification -
            // future MongoDB updates target one channel via arrayFilters matched on type.
            AddError(nameof(Command.Channels), "Channel types must be unique.");
        }

        if (command.Channels.Contains(NotificationChannelType.Push)
            && (command.PushRecipients is null || command.PushRecipients.Count == 0))
        {
            AddError(nameof(Command.PushRecipients), "At least one push recipient token is required when the Push channel is requested.");
        }

        return errors.ToDictionary(pair => pair.Key, pair => pair.Value.ToArray());
    }

    internal static async Task<NotificationDocument> Handle(Command command, MongoContext mongoContext)
    {
        var collection = mongoContext.Database.GetCollection<NotificationDocument>(NotificationDocument.CollectionName);

        var now = DateTime.UtcNow;

        var document = new NotificationDocument
        {
            UserId = command.UserId,
            Email = command.Email,
            Type = command.Type,
            Content = new NotificationContent
            {
                Title = command.ContentTitle,
                Message = command.ContentMessage
            },
            Channels = command.Channels.Select(channelType => BuildChannel(channelType, command.PushRecipients)).ToList(),
            CreatedAt = now,
            UpdatedAt = now
        };

        await collection.InsertOneAsync(document);

        return document;
    }

    internal static NotificationChannel BuildChannel(NotificationChannelType channelType, List<string>? pushRecipients)
    {
        var channel = new NotificationChannel
        {
            Type = channelType,
            Status = NotificationChannelStatus.Pending
        };

        if (channelType == NotificationChannelType.Push)
        {
            channel.Recipients = (pushRecipients ?? [])
                .Select(token => new PushRecipientStatus { Token = token, Status = NotificationChannelStatus.Pending })
                .ToList();
        }

        if (channelType == NotificationChannelType.InApp)
        {
            channel.Read = false;
        }

        return channel;
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailRegex();
}
