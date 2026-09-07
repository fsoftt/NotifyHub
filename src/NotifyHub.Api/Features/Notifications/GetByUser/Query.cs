using System;

namespace NotifyHub.Api.Features.Notifications.GetByUser
{
    public sealed class Query
    {
        public string UserId { get; }
        public int PageSize { get; }
        public string? Cursor { get; }

        public Query(string userId, int pageSize, string? cursor)
        {
            if (userId is null)
            {
                throw new ArgumentNullException(nameof(userId));
            }

            if (pageSize < 0 || pageSize > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize), "PageSize must be between 0 and 100.");
            }

            UserId = userId;
            PageSize = pageSize;
            Cursor = cursor;
        }
    }
}
