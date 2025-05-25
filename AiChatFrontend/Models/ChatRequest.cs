namespace AiChatFrontend.Models;

public class ChatRequest
{
    public List<ChatContent> Previous { get; set; }
    public ChatContent Latest { get; set; }
    public string? ModelId { get; set; }
}
