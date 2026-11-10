namespace RaceDayAPI.Middleware
{
    public static class SessionKeys
    {
        public const string UserId = "UserId";
        public const string UserRole = "UserRole";
        public const string UserEmail = "UserEmail";
    }

    public static class SessionExtensions
    {
        /// <summary>Returns the logged-in user's ID, or null if not authenticated.</summary>
        public static int? GetUserId(this ISession session)
        {
            var value = session.GetString(SessionKeys.UserId);
            return int.TryParse(value, out var id) ? id : null;
        }

        /// <summary>Returns the logged-in user's role, or null if not authenticated.</summary>
        public static string? GetUserRole(this ISession session)
            => session.GetString(SessionKeys.UserRole);

        /// <summary>Returns true when a session with a valid user ID exists.</summary>
        public static bool IsAuthenticated(this ISession session)
            => session.GetUserId() != null;

        /// <summary>Returns true when the session belongs to an Organiser.</summary>
        public static bool IsOrganiser(this ISession session)
            => session.GetUserRole() == "Organiser";

        /// <summary>Returns true when the session belongs to a Participant.</summary>
        public static bool IsParticipant(this ISession session)
            => session.GetUserRole() == "Participant";
    }
}
