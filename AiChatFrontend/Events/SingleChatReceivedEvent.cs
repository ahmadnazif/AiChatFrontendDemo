namespace AiChatFrontend.Events;

public delegate void SingleChatReceivedEventHandler(object sender, SingleChatReceivedEventArgs e);

public class SingleChatReceivedEventArgs(SingleChatResponse response) : EventArgs
{
    public SingleChatResponse Response { get; set; } = response;
}

