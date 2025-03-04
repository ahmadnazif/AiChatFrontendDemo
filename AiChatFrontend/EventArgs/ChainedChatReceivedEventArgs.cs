namespace AiChatFrontend.EventArgs;

public class ChainedChatReceivedEventArgs(ChainedChatResponse response) : System.EventArgs
{
    public ChainedChatResponse Response { get; set; } = response;
}

