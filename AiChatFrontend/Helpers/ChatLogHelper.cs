using Microsoft.AspNetCore.Components;

namespace AiChatFrontend.Helpers;

public static class ChatLogHelper
{
    public static string GetChatCss(ChatLog log)
    {
        return log.Message.Sender switch
        {
            ChatSender.Assistant => "received",
            ChatSender.User => "sent",
            _ => string.Empty,
        };
    }

    public static string GetUsername(ChatLog msg)
    {
        return msg.Message.Sender switch
        {
            ChatSender.User => msg.Username,
            ChatSender.Assistant => msg.Message.Sender.ToString(),
            _ => string.Empty,
        };
    }

    public static MarkupString GetMessageText(ChatLog log)
    {
        return new(log.Message.Text);
    }

    public static string GetChatFooter(ChatLog msg)
    {
        return msg.Message.Sender switch
        {
            ChatSender.Assistant => $"{msg.SentTime.ToLongTimeString()}  |  Time taken: {msg.Duration}  |  Model: {msg.ModelId}",
            ChatSender.User => msg.SentTime.ToLongTimeString(),
            _ => string.Empty,
        };
    }
}
