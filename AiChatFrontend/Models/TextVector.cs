namespace AiChatFrontend.Models;

public class TextVector
{
    public Guid Id { get; set; }
    public string Text { get; set; }
    public ReadOnlyMemory<float> Vector { get; set; }
}
