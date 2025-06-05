using AiChatFrontend.Services;
using Microsoft.AspNetCore.Components;
using Sotsera.Blazor.Toaster;

namespace AiChatFrontend.Pages;

public class TextSimilarityPageBase : ComponentBase
{
    [Inject] public ILogger<TextSimilarityPage> Logger { get; set; }
    [Inject] public ApiClient Api { get; set; }
    [Inject] public IToaster Toastr { get; set; }
    protected List<LlmModel> Models { get; set; } = [];
    protected IEnumerable<string> TextModelIds { get; set; } = [];
    protected string EmbeddingModelId { get; set; } = "Getting..";
    protected string TextModelId { get; set; } = "Getting..";
    protected string MultimodalModelId { get; set; } = "Getting..";
    protected List<TextVector> TextVectors { get; set; } = [];
    protected bool IsStoring { get; set; } = false;
    protected string TextToStore { get; set; } = null;
    protected bool IsComparing { get; set; } = false;
    protected bool IsAutoPopulating { get; set; } = false;
    protected bool IsLlmQuerying { get; set; } = false;
    protected TextSimilarityVectorDbRequest DbReq { get; set; } = new() { Top = 5 };
    protected List<TextSimilarityResult> DbResp { get; set; } = [];
    protected TextSimilarityLlmRequest LlmReq { get; set; } = new();
    protected string LlmResp { get; set; }
    protected string ButtonLabelStore => IsStoring ? "Upserting.." : "Upsert";
    protected string ButtonLabelCompare => IsComparing ? "Processing.." : "Process";
    protected string ButtonLlmQuery => IsLlmQuerying ? "Querying.." : "Query";
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

    #region Step 1
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
    #endregion

    #region Step 2
    protected async Task QueryFromVectorDbAsync()
    {
        if (string.IsNullOrWhiteSpace(DbReq.Prompt))
        {
            Toastr.Warning("Query text is required");
            return;
        }

        IsComparing = true;

        DbResp.Clear();
        var results = Api.StreamTextSimilarityFromDbAsync(DbReq);
        await foreach (var item in results)
        {
            Logger.LogInformation(item.Text);
            DbResp.Add(item);
            StateHasChanged();
        }

        IsComparing = false;
    }
    #endregion

    #region Final
    protected async Task QueryToLlmAsync()
    {
        if (string.IsNullOrWhiteSpace(DbReq.Prompt))
        {
            Toastr.Warning("Query text is required");
            return;
        }

        LlmReq.OriginalPrompt = DbReq.Prompt;
        LlmReq.Results = [.. DbResp.Select(x => x.Text)];
        LlmResp = null;

        var result = Api.StreamTextSimilarityToLlmAsync(LlmReq);
        await foreach(var item in result)
        {
            Logger.LogInformation(item);
            LlmResp += item;
            StateHasChanged();
        }
        
    }
    #endregion

}
