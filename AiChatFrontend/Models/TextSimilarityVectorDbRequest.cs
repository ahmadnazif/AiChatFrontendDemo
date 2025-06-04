namespace AiChatFrontend.Models;

public class TextSimilarityVectorDbRequest
{
    public string? Text { get; set; }
    public int Top { get; set; }
}
