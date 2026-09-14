using System.Globalization;
using System.Text;

namespace NotifyHub.Api.Features.Notifications.List;

internal static class Cursor
{
    internal static string Encode(DateTime createdAt, string id)
    {
        var raw = $"{createdAt:O}|{id}";
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));
    }

    internal static bool TryDecode(string cursor, out DateTime createdAt, out string id)
    {
        createdAt = default;
        id = string.Empty;

        try
        {
            var raw = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
            var parts = raw.Split('|', 2);

            if (parts.Length != 2)
            {
                return false;
            }

            if (!DateTime.TryParse(parts[0], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out createdAt))
            {
                return false;
            }

            if (string.IsNullOrEmpty(parts[1]))
            {
                return false;
            }

            id = parts[1];
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
