using AiChatFrontend.CacheServices;
using AiChatFrontend.Models;
using Markdig.Parsers;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Diagnostics;
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

    public static ChatLog BuildChatLog(string connectionId, string username, string appendedText, StreamingChatResponse last, Stopwatch sw)
    {
        return new()
        {
            ConnectionId = connectionId,
            Username = username,
            Duration = sw.Elapsed.ToString(),
            Message = new(last.Message.Sender, appendedText),
            ModelId = last.ModelId,
            SentTime = last.CreatedAt.LocalDateTime
        };
    }

    public static ChatLog BuildChatLog(SessionCache cache, string appendedText, StreamingChatResponse last, Stopwatch sw)
    {
        return new()
        {
            ConnectionId = cache.Session.ConnectionId,
            Username = cache.Session.Username,
            Duration = sw.Elapsed.ToString(),
            Message = new(last.Message.Sender, appendedText),
            ModelId = last.ModelId,
            SentTime = last.CreatedAt.LocalDateTime
        };
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

    public static string GetChatFooter(KeyValuePair<string, ChatLog> msg)
    {
        var val = msg.Value;
        var duration = string.IsNullOrWhiteSpace(msg.Value.Duration) ? "receiving.." : msg.Value.Duration;
        return val.Message.Sender switch
        {
            ChatSender.Assistant => $"{val.SentTime.ToLongTimeString()}  |  Duration: {duration}  |  Model: {val.ModelId} | Streaming ID: {msg.Key}",
            ChatSender.User => $"{val.SentTime.ToLongTimeString()} | Streaming ID: {msg.Key}",
            _ => string.Empty,
        };
    }

    /// <summary>
    /// <code>
    ///    if (previous.TryGetValue(streamingId, out _))
    ///    {
    ///        previous[streamingId] = current;
    ///    }
    /// </code>
    /// </summary>
    /// <param name="streamingId"></param>
    /// <param name="previous"></param>
    /// <param name="current"></param>
    public static void AppendChatLogs(string streamingId, Dictionary<string, ChatLog> previous, ChatLog current)
    {
        if (previous.TryGetValue(streamingId, out _))
        {
            previous[streamingId] = current;
        }
    }
}
