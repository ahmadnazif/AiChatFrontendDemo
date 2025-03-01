﻿using Markdig.Parsers;
using Microsoft.AspNetCore.Components;
using System.Text.RegularExpressions;

namespace AiChatFrontend.Helpers;

public static class ChatHelper
{
    public static string GetPageTitle(ChatPageType type) => type switch
    {
        ChatPageType.SingleChat => "Generative AI (Single Chat: Fire and Forget)",
        ChatPageType.ChainedChat => "Generative AI (Chained Chat)",
        _ => "-",
    };

    public static List<ChatMsg> BuildPreviousMessages(List<ChatLog> logs)
    {
        List<ChatMsg> list = [];

        foreach (var m in logs)
            list.Add(m.Message);

        return list;
    }

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
        var unescape = Regex.Unescape(log.Message.Text);
        var text = MarkdownHelper.ConvertToHtml(unescape);
        return new(text);
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
