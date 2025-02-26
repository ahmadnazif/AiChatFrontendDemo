namespace AiChatFrontend.EventArgs;

public class MessageReceivedEventArgs(OneChatResponse param) : System.EventArgs
{
    public OneChatResponse Parameter { get; set; } = param;

}
