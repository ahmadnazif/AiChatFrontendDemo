namespace AiChatFrontend.EventArgs;

public class OneChatReceivedEventArgs(SingleChatResponse response) : System.EventArgs
{
    public SingleChatResponse Response { get; set; } = response;
}
