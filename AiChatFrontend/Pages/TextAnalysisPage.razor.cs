using AiChatFrontend.Services;
using Microsoft.AspNetCore.Components;
using Sotsera.Blazor.Toaster;

namespace AiChatFrontend.Pages;

public class TextAnalysisPageBase : ComponentBase
{
    [Inject] public ILogger<TextAnalysisPageBase> Logger { get; set; }
    [Inject] public ApiClient Api { get; set; }
    [Inject] public IToaster Toastr { get; set; }
    protected bool IsApiConnected { get; set; }
    protected static string ApiLastCheck { get; set; } = DateTime.Now.ToLongTimeString();
    protected List<LlmModel> Models { get; set; } = [];
    protected IEnumerable<string> TextModelIds { get; set; } = [];
    protected string EmbeddingModelId { get; set; } = "Getting..";
    protected string TextModelId { get; set; } = "Getting..";
    protected string MultimodalModelId { get; set; } = "Getting..";
    protected List<TextVector> TextVectors { get; set; } = [];
    protected bool IsStoring { get; set; } = false;
    protected string TextToStore { get; set; } = null;
    protected bool IsAutoPopulating { get; set; } = false;
    protected bool IsVdbQuerying { get; set; } = false;
    protected bool IsLlmQuerying { get; set; } = false;
    protected VdbRequest VdbReq { get; set; } = new() { Top = 1 };
    protected List<TextAnalysisVdbQueryResult> VdbResp { get; set; } = [];
    protected LlmRequest LlmReq { get; set; } = new();
    protected string LlmResp { get; set; } = "Please initiate the query";
    protected string ButtonStore => IsStoring ? "Upserting.." : "Upsert";
    protected string ButtonVdbQuery => IsVdbQuerying ? "Querying.." : "Query";
    protected string ButtonLlmQuery => IsLlmQuerying ? "Querying.." : "Query";
    protected string ButtonAutoPopulate => IsAutoPopulating ? "Generating.." : "Generate & Upsert";
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
        await RefreshApiStatusAsync();
        await RefreshModelsAsync();
        await RefreshVectorsAsync();
    }

    protected async Task RefreshApiStatusAsync()
    {
        IsApiConnected = await Api.IsConnectedAsync();
        ApiLastCheck = DateTime.Now.ToLongTimeString();
    }

    private async Task RefreshModelsAsync()
    {
        Models = await Api.ListAllModelsAsync();
        EmbeddingModelId = LlmModelHelper.GetDefaulModelId(Models, LlmModelType.Embedding);
        TextModelId = LlmModelHelper.GetDefaulModelId(Models, LlmModelType.Text);
        TextModelIds = LlmModelHelper.GetModelIds(Models, LlmModelType.Text);
        MultimodalModelId = LlmModelHelper.GetDefaulModelId(Models, LlmModelType.Multimodal);
    }

    private async Task RefreshVectorsAsync()
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
            await RefreshVectorsAsync();
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
            await RefreshVectorsAsync();
        }
        else
            Toastr.Error(resp.Message);
    }

    protected async Task AutoPopulateAsync()
    {
        IsAutoPopulating = true;

        var resp = await Api.AutoPopulateStatementToDbAsync(AutoPopulateRequest);
        if (resp.IsSuccess)
        {
            Toastr.Success(resp.Message);
            await RefreshVectorsAsync();
        }
        else
            Toastr.Error(resp.Message);

        IsAutoPopulating = false;
    }
   
    #endregion

    #region Step 2

    protected async Task QueryVectorDbAsync()
    {
        if (string.IsNullOrWhiteSpace(VdbReq.Prompt))
        {
            Toastr.Warning("Query text is required");
            return;
        }

        IsVdbQuerying = true;

        VdbResp.Clear();
        var results = Api.StreamTextAnalysisVdbAsync(VdbReq);
        await foreach (var item in results)
        {
            VdbResp.Add(item);
            StateHasChanged();
        }

        IsVdbQuerying = false;
    }
    
    #endregion

    #region Final

    private CancellationTokenSource ctsQueryLlm;

    protected async Task QueryLlmAsync()
    {
        if (string.IsNullOrWhiteSpace(VdbReq.Prompt))
        {
            Toastr.Warning("Query text is required");
            return;
        }

        ctsQueryLlm = new();

        LlmReq.OriginalPrompt = VdbReq.Prompt;
        LlmReq.Results = [.. VdbResp.Select(x => x.Text)];

        IsLlmQuerying = true;
        LlmResp = "Querying..";

        var stream = Api.StreamTextAnalysisLlmAsync(LlmReq, ctsQueryLlm.Token);
        LlmResp = string.Empty;
        await foreach (var item in stream)
        {
            LlmResp += item.Message.Text;
            IsLlmQuerying = !item.HasFinished;
            StateHasChanged();
        }
    }

    protected void StopQueryLlm()
    {
        ctsQueryLlm.Cancel();
        IsLlmQuerying = false;
    }
   
    #endregion
}
