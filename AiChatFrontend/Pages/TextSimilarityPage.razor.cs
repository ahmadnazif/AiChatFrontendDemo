using AiChatFrontend.Services;
using Microsoft.AspNetCore.Components;

namespace AiChatFrontend.Pages;

public class TextSimilarityPageBase : ComponentBase
{
    [Inject] public ApiClient Api { get; set; }
    protected List<TextVector> TextVectors { get; set; } = [];
    protected override async Task OnInitializedAsync()
    {
        TextVectors = await Api.ListAllTextVectorFromCacheAsync();
    }
}
