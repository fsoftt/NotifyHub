using FluentAssertions;
using NotifyHub.Api.Features.Notifications.Create;
using NotifyHub.Api.Infrastructure.Notifications;
using Xunit;

namespace NotifyHub.Api.Tests.Features.Notifications.Create;

public class HandlerTests
{
    private static Command ValidCommand(
        List<NotificationChannelType>? channels = null,
        List<string>? pushRecipients = null) =>
        new(
            UserId: null,
            Email: "user@example.com",
            Type: "ScoreAssigned",
            ContentTitle: "New score",
            ContentMessage: "A new score has been assigned to you.",
            Channels: channels ?? [NotificationChannelType.Email],
            PushRecipients: pushRecipients);

    [Fact]
    public void Validate_WithAllFieldsValid_ReturnsNoErrors()
    {
        var command = ValidCommand(
            channels: [NotificationChannelType.Email, NotificationChannelType.Push],
            pushRecipients: ["device-token-1"]);

        var errors = Handler.Validate(command);

        errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-an-email")]
    [InlineData("missing-at-sign.com")]
    public void Validate_WithInvalidEmail_ReturnsEmailError(string email)
    {
        var command = ValidCommand() with { Email = email };

        var errors = Handler.Validate(command);

        errors.Should().ContainKey(nameof(Command.Email));
    }

    [Fact]
    public void Validate_WithMissingType_ReturnsTypeError()
    {
        var command = ValidCommand() with { Type = "" };

        var errors = Handler.Validate(command);

        errors.Should().ContainKey(nameof(Command.Type));
    }

    [Fact]
    public void Validate_WithMissingContentTitle_ReturnsContentTitleError()
    {
        var command = ValidCommand() with { ContentTitle = "" };

        var errors = Handler.Validate(command);

        errors.Should().ContainKey(nameof(Command.ContentTitle));
    }

    [Fact]
    public void Validate_WithMissingContentMessage_ReturnsContentMessageError()
    {
        var command = ValidCommand() with { ContentMessage = "" };

        var errors = Handler.Validate(command);

        errors.Should().ContainKey(nameof(Command.ContentMessage));
    }

    [Fact]
    public void Validate_WithNoChannels_ReturnsChannelsError()
    {
        var command = ValidCommand(channels: []);

        var errors = Handler.Validate(command);

        errors.Should().ContainKey(nameof(Command.Channels));
    }

    [Fact]
    public void Validate_WithDuplicateChannelTypes_ReturnsChannelsError()
    {
        var command = ValidCommand(channels: [NotificationChannelType.Email, NotificationChannelType.Email]);

        var errors = Handler.Validate(command);

        errors.Should().ContainKey(nameof(Command.Channels));
    }

    [Fact]
    public void Validate_WithPushChannelAndNoPushRecipients_ReturnsPushRecipientsError()
    {
        var command = ValidCommand(channels: [NotificationChannelType.Push], pushRecipients: null);

        var errors = Handler.Validate(command);

        errors.Should().ContainKey(nameof(Command.PushRecipients));
    }

    [Fact]
    public void Validate_WithPushChannelAndEmptyPushRecipients_ReturnsPushRecipientsError()
    {
        var command = ValidCommand(channels: [NotificationChannelType.Push], pushRecipients: []);

        var errors = Handler.Validate(command);

        errors.Should().ContainKey(nameof(Command.PushRecipients));
    }

    [Fact]
    public void BuildChannel_ForPush_AttachesOneRecipientPerToken()
    {
        var channel = Handler.BuildChannel(
            NotificationChannelType.Push,
            ["device-token-1", "device-token-2"]);

        channel.Recipients.Should().HaveCount(2);
        channel.Recipients!.Select(recipient => recipient.Token)
            .Should().BeEquivalentTo("device-token-1", "device-token-2");
        channel.Recipients!.Should().OnlyContain(recipient => recipient.Status == NotificationChannelStatus.Pending);
    }

    [Fact]
    public void BuildChannel_ForPush_WithNoRecipientsGiven_ProducesEmptyListNotNull()
    {
        var channel = Handler.BuildChannel(NotificationChannelType.Push, pushRecipients: null);

        channel.Recipients.Should().NotBeNull();
        channel.Recipients.Should().BeEmpty();
    }

    [Fact]
    public void BuildChannel_ForInApp_DefaultsReadToFalse()
    {
        var channel = Handler.BuildChannel(NotificationChannelType.InApp, pushRecipients: null);

        channel.Read.Should().BeFalse();
        channel.Recipients.Should().BeNull();
    }

    [Fact]
    public void BuildChannel_ForEmail_HasNeitherRecipientsNorReadState()
    {
        var channel = Handler.BuildChannel(NotificationChannelType.Email, pushRecipients: null);

        channel.Recipients.Should().BeNull();
        channel.Read.Should().BeNull();
    }

    [Fact]
    public void BuildChannel_AlwaysStartsPending()
    {
        var channel = Handler.BuildChannel(NotificationChannelType.Email, pushRecipients: null);

        channel.Status.Should().Be(NotificationChannelStatus.Pending);
    }
}
