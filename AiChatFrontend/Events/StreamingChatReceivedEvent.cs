namespace AiChatFrontend.Events;

public delegate void StreamingChatReceivedEventHandler(object sender, StreamingChatReceivedEventArgs e);

public class StreamingChatReceivedEventArgs(StreamingChatResponse response) : EventArgs
{
    public StreamingChatResponse Response { get; set; } = response;
}