using System.Text;
using System.Text.Json;

namespace NotifyHub.Api.Features.Notifications.GetByUser
{
    public static class CursorEncoder
    {
        public static string Encode(Cursor cursor)
        {
            var json = JsonSerializer.Serialize(cursor);

            return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
        }

        public static Cursor? Decode(string? encodedCursor)
        {
            if (string.IsNullOrWhiteSpace(encodedCursor))
            {
                return null;
            }

            try
            {
                var bytes = Convert.FromBase64String(encodedCursor);

                var json = Encoding.UTF8.GetString(bytes);

                return JsonSerializer.Deserialize<Cursor>(json);
            }
            catch (FormatException)
            {
                return null;
            }
            catch (JsonException)
            {
                return null;
            }
        }
    }
}
