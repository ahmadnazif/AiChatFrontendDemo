namespace AiChatFrontend.Models;

public class ChainedChatRequest
{
    public List<ChatMsg> PreviousMessages { get; set; }
    public ChatMsg LatestMessage { get; set; }
}
