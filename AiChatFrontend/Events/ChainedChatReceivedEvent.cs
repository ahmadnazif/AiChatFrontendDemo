namespace AiChatFrontend.Events;

public delegate void ChainedChatReceivedEventHandler(object sender, ChainedChatReceivedEventArgs e);

public class ChainedChatReceivedEventArgs(ChainedChatResponse response) : System.EventArgs
{
    public ChainedChatResponse Response { get; set; } = response;
}

