namespace AiChatFrontend.Models;

public class ChatFile
{
    public string? FileName { get; set; }
    public byte[] FileStream { get; set; }
    public string? MediaType { get; set; }
}
