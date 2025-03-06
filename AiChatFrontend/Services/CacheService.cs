namespace AiChatFrontend.Services;

public class CacheService
{
    public UserSession Session { get; private set; } = null;

    public void StartSession(UserSession session)
    {
        Session = session;
    }

    public bool IsAuthenticated => Session != null;

    public void RemoveSession() => Session = null;
}
