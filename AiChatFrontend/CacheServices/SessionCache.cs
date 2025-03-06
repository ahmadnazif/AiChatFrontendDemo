namespace AiChatFrontend.CacheServices;

public class SessionCache
{
    public UserSession Session { get; private set; } = null;

    public void StartSession(UserSession session)
    {
        Session = session;
    }

    public bool IsAuthenticated => Session != null;

    public void RemoveSession() => Session = null;
}
