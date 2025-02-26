using Microsoft.AspNetCore.Components;

namespace AiChatFrontend.Pages;

public class ChatPageBase : ComponentBase, IAsyncDisposable
{
    public ValueTask DisposeAsync()
    {
        throw new NotImplementedException();
    }
}
