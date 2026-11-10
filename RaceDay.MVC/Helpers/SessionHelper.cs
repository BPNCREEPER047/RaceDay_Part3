namespace RaceDay.MVC.Helpers
{
    public static class SessionHelper
    {
        public const string UserIdKey  = "user_id";
        public const string RoleKey    = "user_role";
        public const string NameKey    = "user_name";
        public const string EmailKey   = "user_email";

        public static void SetUser(ISession session,
            int id, string role, string name, string email)
        {
            session.SetString(UserIdKey, id.ToString());
            session.SetString(RoleKey,   role);
            session.SetString(NameKey,   name);
            session.SetString(EmailKey,  email);
        }

        public static void ClearUser(ISession session) => session.Clear();

        public static string? GetRole(ISession session)  => session.GetString(RoleKey);
        public static string? GetName(ISession session)  => session.GetString(NameKey);
        public static string? GetEmail(ISession session) => session.GetString(EmailKey);
        public static int?    GetUserId(ISession session)
        {
            var v = session.GetString(UserIdKey);
            return v == null ? null : int.Parse(v);
        }

        public static bool IsLoggedIn(ISession session)    => session.GetString(UserIdKey) != null;
        public static bool IsOrganiser(ISession session)   => GetRole(session) == "Organiser";
        public static bool IsParticipant(ISession session) => GetRole(session) == "Participant";
    }
}
