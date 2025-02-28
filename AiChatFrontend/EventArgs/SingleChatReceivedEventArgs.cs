namespace AiChatFrontend.EventArgs;

public class SingleChatReceivedEventArgs(SingleChatResponse response) : System.EventArgs
{
    public SingleChatResponse Response { get; set; } = response;
}
