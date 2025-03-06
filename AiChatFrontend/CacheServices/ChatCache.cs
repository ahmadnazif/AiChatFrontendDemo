namespace AiChatFrontend.CacheServices;

public class ChatCache
{
    public Dictionary<string, ChatLog> ChatLogs { get; private set; } = [];

    public int Count => ChatLogs.Count;

    public void Add(string streamingId, ChatLog log)
    {
        ChatLogs.Add(streamingId, log);
    }

    public void Clear()
    {
        ChatLogs.Clear();
    }

}
