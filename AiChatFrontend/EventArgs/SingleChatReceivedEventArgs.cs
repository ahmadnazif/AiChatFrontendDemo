namespace AiChatFrontend.EventArgs;

public delegate void SingleChatReceivedEventHandler(object sender, SingleChatReceivedEventArgs e);

public class SingleChatReceivedEventArgs(SingleChatResponse response) : System.EventArgs
{
    public SingleChatResponse Response { get; set; } = response;
}
