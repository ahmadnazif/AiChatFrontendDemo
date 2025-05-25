namespace AiChatFrontend.Models;

public class LlmModel
{
    public LlmModelType ModelType { get; set; }
    public string ModelTypeName => ModelType.ToString();
    public List<string> Models { get; set; }
    public string? DefaultModel { get; set; }
}
