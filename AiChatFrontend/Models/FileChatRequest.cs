namespace AiChatFrontend.Models;

public class FileChatRequest : ChatFileTemp
{
    public ChatMsg Prompt { get; set; }
    public string? ModelId { get; set; }
}
