using Markdig;

namespace AiChatFrontend.Helpers;

public static class MarkdownHelper
{
    public static string ConvertToHtml(string markdown)
    {
        return Markdown.ToHtml(markdown);
    }
}
