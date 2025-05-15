using AiChatFrontend.Services;
using Microsoft.AspNetCore.Components;
using Sotsera.Blazor.Toaster;

namespace AiChatFrontend.Pages;

public class TextSimilarityPageBase : ComponentBase
{
    [Inject] public ApiClient Api { get; set; }
    [Inject] public IToaster Toastr { get; set; }
    protected string EmbeddingModelName { get; set; } = "Getting..";
    protected string TextModelName { get; set; } = "Getting..";
    protected List<TextVector> TextVectors { get; set; } = [];
    protected List<TextSimilarityResult> TextSimilarityResults { get; set; } = [];
    protected string TextToStore { get; set; } = null;
    protected bool IsStoring { get; set; } = false;
    protected string TextToCompare { get; set; } = null;
    protected bool IsComparing { get; set; } = false;
    protected bool IsAutopopulating { get; set; } = false;
    protected string ButtonLabelStore => IsStoring ? "Upserting.." : "Upsert";
    protected string ButtonLabelCompare => IsComparing ? "Processing.." : "Process";
    protected string ButtonLabelAutopopulateStatement => IsAutopopulating ? "Generating.." : "Generate";
    protected int AutoPopulateNumber { get; set; } = 5;
    protected override async Task OnInitializedAsync()
    {
        await RefreshTextVectorAsync();
    }

    private async Task RefreshTextVectorAsync()
    {
        EmbeddingModelName = await Api.GetModelNameAsync(LlmModelType.Embedding);
        TextModelName = await Api.GetModelNameAsync(LlmModelType.Text);
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
        IsAutopopulating = true;
        var resp = await Api.AutoPopulateStatementToDbAsync(AutoPopulateNumber);
        if (resp.IsSuccess)
        {
            Toastr.Success(resp.Message);
            await RefreshTextVectorAsync();
        }
        else
            Toastr.Error(resp.Message);

        IsAutopopulating = false;
    }


    protected async Task CompareTextAsync()
    {
        if (string.IsNullOrWhiteSpace(TextToCompare))
        {
            Toastr.Warning("Text to compare is required");
            return;
        }

        IsComparing = true;

        TextSimilarityResults.Clear();
        var result = Api.StreamTextVectorSimilarityAsync(TextToCompare);
        await foreach (var r in result)
        {
            TextSimilarityResults.Add(r);
            StateHasChanged();
        }

        IsComparing = false;
    }

}
