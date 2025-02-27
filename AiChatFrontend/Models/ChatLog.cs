using AiChatFrontend.Enums;

namespace AiChatFrontend.Models;

public class ChatLog
{
    public string Duration { get; set; }
    public string ModelId { get; set; }
    public string Username { get; set; }
    public string ConnectionId { get; set; }
    public ChatMsg Message { get; set; }
    public DateTime SentTime { get; set; }
}
