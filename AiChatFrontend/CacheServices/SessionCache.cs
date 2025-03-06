namespace AiChatFrontend.CacheServices;

public class SessionCache
{
    public UserSession Session { get; private set; } = null;

    public void Start(UserSession session)
    {
        Session = session;
    }

    public bool IsAuthenticated => Session != null;

    public void Remove() => Session = null;
}
