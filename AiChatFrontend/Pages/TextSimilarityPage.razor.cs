using AiChatFrontend.Services;
using Microsoft.AspNetCore.Components;
using Sotsera.Blazor.Toaster;

namespace AiChatFrontend.Pages;

public class TextSimilarityPageBase : ComponentBase
{
    [Inject] public ApiClient Api { get; set; }
    [Inject] public IToaster Toastr { get; set; }
    protected List<LlmModel> Models { get; set; } = [];
    protected IEnumerable<string> TextModelIds { get; set; } = [];
    protected string EmbeddingModelId { get; set; } = "Getting..";
    protected string TextModelId { get; set; } = "Getting..";
    protected string MultimodalModelId { get; set; } = "Getting..";
    protected List<TextVector> TextVectors { get; set; } = [];
    protected List<TextSimilarityResult> TextSimilarityResults { get; set; } = [];
    protected bool IsStoring { get; set; } = false;
    protected string TextToStore { get; set; } = null;
    protected bool IsComparing { get; set; } = false;
    protected bool IsAutoPopulating { get; set; } = false;
    protected TextSimilarityPrompt Prompt { get; set; } = new() { Top = 5 };
    protected string ButtonLabelStore => IsStoring ? "Upserting.." : "Upsert";
    protected string ButtonLabelCompare => IsComparing ? "Processing.." : "Process";
    protected string ButtonLabelAutoPopulate => IsAutoPopulating ? "Generating.." : "Generate & Upsert";
    protected AutoPopulateStatementRequest AutoPopulateRequest { get; set; } = new()
    {
        ModelId = string.Empty,
        Number = 5,
        Length = TextGenerationLength.Shortest,
        Topic = "Random",
        Language = "English"
    };

    protected override async Task OnInitializedAsync()
    {
        await RefreshModelsAsync();
        await RefreshTextVectorAsync();
    }

    private async Task RefreshModelsAsync()
    {
        Models = await Api.ListAllModelsAsync();
        EmbeddingModelId = LlmModelHelper.GetDefaulModelId(Models, LlmModelType.Embedding);
        TextModelId = LlmModelHelper.GetDefaulModelId(Models, LlmModelType.Text);
        TextModelIds = LlmModelHelper.GetModelIds(Models, LlmModelType.Text);
        MultimodalModelId = LlmModelHelper.GetDefaulModelId(Models, LlmModelType.Multimodal);
    }

    private async Task RefreshTextVectorAsync()
    {
        TextVectors = await Api.ListAllTextVectorFromCacheAsync();
    }

    protected async Task StoreTextAsync()
    {
        if (string.IsNullOrWhiteSpace(TextToStore))
        {
            Toastr.Warning("Text to feed is required");
            return;
        }

        IsStoring = true;
        var resp = await Api.StoreTextVectorToDbAsync(TextToStore);
        if (resp.IsSuccess)
        {
            Toastr.Success(resp.Message);
            await RefreshTextVectorAsync();
        }
        else
            Toastr.Error(resp.Message);

        IsStoring = false;
    }

    protected async Task DeleteTextAsync(Guid key)
    {
        var resp = await Api.DeleteTextVectorFromDbAsync(key.ToString());
        if (resp.IsSuccess)
        {
            Toastr.Success($"Item {key} deleted");
            await RefreshTextVectorAsync();
        }
        else
            Toastr.Error(resp.Message);
    }

    protected async Task AutoPopulateStatementAsync()
    {
        IsAutoPopulating = true;

        var resp = await Api.AutoPopulateStatementToDbAsync(AutoPopulateRequest);
        if (resp.IsSuccess)
        {
            Toastr.Success(resp.Message);
            await RefreshTextVectorAsync();
        }
        else
            Toastr.Error(resp.Message);

        IsAutoPopulating = false;
    }


    protected async Task CompareTextAsync()
    {
        if (string.IsNullOrWhiteSpace(Prompt.Text))
        {
            Toastr.Warning("Text to compare is required");
            return;
        }

        IsComparing = true;

        TextSimilarityResults.Clear();
        var result = Api.StreamTextVectorSimilarityAsync(Prompt);
        await foreach (var r in result)
        {
            TextSimilarityResults.Add(r);
            StateHasChanged();
        }

        IsComparing = false;
    }

}
