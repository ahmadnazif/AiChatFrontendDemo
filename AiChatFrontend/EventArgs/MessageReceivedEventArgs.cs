namespace AiChatFrontend.EventArgs;

public class MessageReceivedEventArgs(ChatHubChatResponse param) : System.EventArgs
{
    public ChatHubChatResponse Parameter { get; set; } = param;

}
