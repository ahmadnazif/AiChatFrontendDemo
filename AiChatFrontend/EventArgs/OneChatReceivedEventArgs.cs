namespace AiChatFrontend.EventArgs;

public class OneChatReceivedEventArgs(OneChatResponse response) : System.EventArgs
{
    public OneChatResponse Response { get; set; } = response;
}
