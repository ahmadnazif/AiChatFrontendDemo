using AiChatFrontend.Enums;

namespace AiChatFrontend.Models;

public class ChatMessage : ChatMessageBase
{
    public ChatSender Sender { get; set; }
    public string Duration { get; set; }
    public string ModelId { get; set; }
}

public class ChatMessageBase
{
    public string Username { get; set; }
    public string ConnectionId { get; set; }
    public string Message { get; set; }
    public DateTime SentTime { get; set; }
}
