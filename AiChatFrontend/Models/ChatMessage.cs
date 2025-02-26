namespace AiChatFrontend.Models;

public class ChatMessage : ChatMessageBase
{
    public bool IsMine { get; set; }
}

public class ChatMessageBase
{
    public string Username { get; set; }
    public string ConnectionId { get; set; }
    public string Message { get; set; }
    public DateTime SentTime { get; set; }
}
