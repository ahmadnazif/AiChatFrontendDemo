namespace AiChatFrontend.CacheServices;

public class FileChatCache
{
    [Obsolete("Use ChatLogsNew")] public Dictionary<string, ChatLog> ChatLogs { get; private set; } = [];
    public Dictionary<string, ChatLogNew> ChatLogsNew { get; private set; } = [];

    [Obsolete] public int Count => ChatLogs.Count;
    public int CountNew => ChatLogsNew.Count;

    [Obsolete]
    public void TryAdd(string streamingId, ChatLog log)
    {
        if (string.IsNullOrWhiteSpace(streamingId))
            return;

        if (log == null)
            return;

        ChatLogs.TryAdd(streamingId, log);
    }

    public void TryAddNew(string streamingId, ChatLogNew log)
    {
        if (string.IsNullOrWhiteSpace(streamingId))
            return;

        if (log == null)
            return;

        ChatLogsNew.TryAdd(streamingId, log);
    }

    [Obsolete]
    public bool Remove(string streamingId)
    {
        if (string.IsNullOrWhiteSpace(streamingId))
            return false;

        return ChatLogs.Remove(streamingId);
    }

    public bool RemoveNew(string streamingId)
    {
        if (string.IsNullOrWhiteSpace(streamingId))
            return false;

        return ChatLogsNew.Remove(streamingId);
    }

    [Obsolete] public void Clear() => ChatLogs.Clear();
    public void ClearNew() => ChatLogsNew.Clear();

}
