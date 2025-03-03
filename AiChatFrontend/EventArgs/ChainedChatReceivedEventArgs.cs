namespace AiChatFrontend.EventArgs;

public class ChainedChatReceivedEventArgs(ChainedChatResponse response) : System.EventArgs
{
    public ChainedChatResponse Response { get; set; } = response;
}

// TODO: move out
public class StreamingChatReceivedEventArgs(string response) : System.EventArgs
{
    public string? Response { get; set; } = response;
}

