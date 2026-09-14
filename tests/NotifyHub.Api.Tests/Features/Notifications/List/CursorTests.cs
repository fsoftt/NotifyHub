using FluentAssertions;
using NotifyHub.Api.Features.Notifications.List;
using Xunit;

namespace NotifyHub.Api.Tests.Features.Notifications.List;

public class CursorTests
{
    [Fact]
    public void EncodeThenTryDecode_RoundTripsToTheSameValues()
    {
        var createdAt = new DateTime(2026, 9, 14, 21, 49, 58, 200, DateTimeKind.Utc);
        var id = "6aa86c06d54d73d4eaa7b8d1";

        var cursor = Cursor.Encode(createdAt, id);
        var decoded = Cursor.TryDecode(cursor, out var decodedCreatedAt, out var decodedId);

        decoded.Should().BeTrue();
        decodedCreatedAt.Should().Be(createdAt);
        decodedId.Should().Be(id);
    }

    [Fact]
    public void TryDecode_WithInvalidBase64_ReturnsFalse()
    {
        var decoded = Cursor.TryDecode("not-valid-base64!!!", out _, out _);

        decoded.Should().BeFalse();
    }

    [Fact]
    public void TryDecode_WithMissingSeparator_ReturnsFalse()
    {
        var malformed = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("no-separator-here"));

        var decoded = Cursor.TryDecode(malformed, out _, out _);

        decoded.Should().BeFalse();
    }

    [Fact]
    public void TryDecode_WithUnparseableDate_ReturnsFalse()
    {
        var malformed = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("not-a-date|some-id"));

        var decoded = Cursor.TryDecode(malformed, out _, out _);

        decoded.Should().BeFalse();
    }

    [Fact]
    public void TryDecode_WithEmptyId_ReturnsFalse()
    {
        var malformed = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("2026-09-14T21:49:58.2000000Z|"));

        var decoded = Cursor.TryDecode(malformed, out _, out _);

        decoded.Should().BeFalse();
    }
}
