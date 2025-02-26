namespace AiChatFrontend.Models;

public class ChatHubBatchChatRequest
{
    public List<ChatRequest> Messages { get; set; }

}

public record ChatRequest(ChatSender Sender, string Message);

