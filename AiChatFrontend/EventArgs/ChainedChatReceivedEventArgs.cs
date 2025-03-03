namespace AiChatFrontend.EventArgs;

public class ChainedChatReceivedEventArgs(ChainedChatResponse response) : System.EventArgs
{
    public ChainedChatResponse Response { get; set; } = response;
}

// TODO: move out
public class StreamingChatReceivedEventArgs(StreamingChatResponse response) : System.EventArgs
{
    public StreamingChatResponse Response { get; set; } = response;
}

