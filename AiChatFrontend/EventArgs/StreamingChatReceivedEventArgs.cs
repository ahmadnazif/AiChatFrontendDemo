namespace AiChatFrontend.EventArgs;

public class StreamingChatReceivedEventArgs(StreamingChatResponse response) : System.EventArgs
{
    public StreamingChatResponse Response { get; set; } = response;
}