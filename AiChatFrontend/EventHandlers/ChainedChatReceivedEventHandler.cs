using AiChatFrontend.EventArgs;

namespace AiChatFrontend.EventHandlers;

public delegate void ChainedChatReceivedEventHandler(object sender, ChainedChatReceivedEventArgs e);

// TODO: move out
public delegate void StreamingChatReceivedEventHandler(object sender, StreamingChatReceivedEventArgs e);

