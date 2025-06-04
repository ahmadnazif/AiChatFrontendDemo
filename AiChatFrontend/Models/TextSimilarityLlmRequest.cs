namespace AiChatFrontend.Models;

public class TextSimilarityLlmRequest
{
    public string? OriginalPrompt { get; set; }
    public string? ModelId { get; set; }
}
