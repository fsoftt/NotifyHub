using FluentAssertions;
using NotifyHub.Api.Infrastructure.Notifications;
using Xunit;
using CreateEndpoint = NotifyHub.Api.Features.Notifications.Create.Endpoint;

namespace NotifyHub.Api.Tests.Features.Notifications.Create;

public class EndpointTests
{
    [Fact]
    public void BuildChannel_ForPush_AttachesOneRecipientPerToken()
    {
        var channel = CreateEndpoint.BuildChannel(
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
        var channel = CreateEndpoint.BuildChannel(NotificationChannelType.Push, pushRecipients: null);

        channel.Recipients.Should().NotBeNull();
        channel.Recipients.Should().BeEmpty();
    }

    [Fact]
    public void BuildChannel_ForInApp_DefaultsReadToFalse()
    {
        var channel = CreateEndpoint.BuildChannel(NotificationChannelType.InApp, pushRecipients: null);

        channel.Read.Should().BeFalse();
        channel.Recipients.Should().BeNull();
    }

    [Fact]
    public void BuildChannel_ForEmail_HasNeitherRecipientsNorReadState()
    {
        var channel = CreateEndpoint.BuildChannel(NotificationChannelType.Email, pushRecipients: null);

        channel.Recipients.Should().BeNull();
        channel.Read.Should().BeNull();
    }

    [Fact]
    public void BuildChannel_AlwaysStartsPending()
    {
        var channel = CreateEndpoint.BuildChannel(NotificationChannelType.Email, pushRecipients: null);

        channel.Status.Should().Be(NotificationChannelStatus.Pending);
    }
}
