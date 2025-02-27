namespace AiChatFrontend.EventArgs;

public class MessageReceivedEventArgs(OneChatResponse response) : System.EventArgs
{
    public OneChatResponse Response { get; set; } = response;
}
