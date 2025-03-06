namespace AiChatFrontend.CacheServices;

public class ChatCache
{
    public Dictionary<string, ChatLog> ChatLogs { get; private set; } = [];

    public int Count => ChatLogs.Count;

    public void Add(string streamingId, ChatLog log)
    {
        if (string.IsNullOrWhiteSpace(streamingId))
            return;

        if (log == null)
            return;

        ChatLogs.Add(streamingId, log);
    }

    public bool Remove(string streamingId)
    {
        if (string.IsNullOrWhiteSpace(streamingId))
            return false;

        return ChatLogs.Remove(streamingId);
    }

    public void Clear()
    {
        ChatLogs.Clear();
    }

}
