using AiChatFrontend.Services;
using Microsoft.AspNetCore.Components;
using Sotsera.Blazor.Toaster;

namespace AiChatFrontend.Pages;

public class RecipeAnalysisPageBase : ComponentBase
{
    [Inject] public ILogger<RecipeAnalysisPageBase> Logger { get; set; }
    [Inject] public ApiClient Api { get; set; }
    [Inject] public IToaster Toastr { get; set; }
    protected bool IsApiConnected { get; set; }
    protected bool IsQdrantConnected { get; set; }
    protected List<LlmModel> Models { get; set; } = [];
    protected IEnumerable<string> TextModelIds { get; set; } = [];
    protected string EmbeddingModelId { get; set; } = "Getting..";
    protected string TextModelId { get; set; } = "Getting..";
    protected string MultimodalModelId { get; set; } = "Getting..";

    protected async override Task OnInitializedAsync()
    {
        IsApiConnected = await Api.IsConnectedAsync();
        IsQdrantConnected = await Api.IsQdrantRunningAsync();

        await RefreshModelsAsync();
    }

    private async Task RefreshModelsAsync()
    {
        Models = await Api.ListAllModelsAsync();
        EmbeddingModelId = LlmModelHelper.GetDefaulModelId(Models, LlmModelType.Embedding);
        TextModelId = LlmModelHelper.GetDefaulModelId(Models, LlmModelType.Text);
        TextModelIds = LlmModelHelper.GetModelIds(Models, LlmModelType.Text);
        MultimodalModelId = LlmModelHelper.GetDefaulModelId(Models, LlmModelType.Multimodal);
    }
}
