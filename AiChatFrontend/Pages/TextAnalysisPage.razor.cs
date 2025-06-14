using AiChatFrontend.Services;
using Microsoft.AspNetCore.Components;
using Sotsera.Blazor.Toaster;

namespace AiChatFrontend.Pages;

public class TextAnalysisPageBase : Component
{
    protected bool IsApiConnected { get; set; }
    protected static string ApiLastCheck { get; set; } = DateTime.Now.ToLongTimeString();
    protected List<LlmModel> Models { get; set; } = [];
    protected IEnumerable<string> TextModelIds { get; set; } = [];
    protected string EmbeddingModelId { get; set; } = "Getting..";
    protected string MultimodalModelId { get; set; } = "Getting..";

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
        TextModelIds = LlmModelHelper.GetModelIds(Models, LlmModelType.Text);
        MultimodalModelId = LlmModelHelper.GetDefaulModelId(Models, LlmModelType.Multimodal);
    }


    #region Step 1
    protected List<TextVector> TextVectors { get; set; } = [];
    protected bool IsUpserting { get; set; } = false;
    protected string TextToStore { get; set; } = null;
    protected bool IsAutoPopulating { get; set; } = false;
    protected string ButtonUpsert => IsUpserting ? "Upserting.." : "Upsert";
    protected string ButtonAutoPopulate => IsAutoPopulating ? "Generating.." : "Generate & Upsert";
    protected AutoPopulateStatementRequest AutoPopulateRequest { get; set; } = new()
    {
        ModelId = string.Empty,
        Number = 5,
        Length = TextGenerationLength.Shortest,
        Topic = "Random",
        Language = "English"
    };

    private async Task RefreshVectorsAsync()
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

        IsUpserting = true;
        var resp = await Api.StoreTextVectorToDbAsync(TextToStore);
        if (resp.IsSuccess)
        {
            Toastr.Success(resp.Message);
            await RefreshVectorsAsync();
        }
        else
            Toastr.Error(resp.Message);

        IsUpserting = false;
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
    protected bool IsVdbQuerying { get; set; } = false;
    protected VdbRequest VdbReq { get; set; } = new() { Top = 1 };
    protected string ButtonVdbQuery => IsVdbQuerying ? "Querying.." : "Query";
    protected List<TextAnalysisVdbQueryResult> VdbResp { get; set; } = [];

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
    protected bool IsLlmQuerying { get; set; } = false;
    protected LlmRequest LlmReq { get; set; } = new();
    protected string LlmResp { get; set; } = "Please initiate the query";
    protected string ButtonLlmQuery => IsLlmQuerying ? "Querying.." : "Query";

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
