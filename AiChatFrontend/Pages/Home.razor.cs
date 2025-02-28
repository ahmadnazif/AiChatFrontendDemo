using AiChatFrontend.Services;
using Microsoft.AspNetCore.Components;
using Sotsera.Blazor.Toaster;
using System.Diagnostics;

namespace AiChatFrontend.Pages;

public class HomeBase : ComponentBase
{
    [Inject] public ApiClient Api { get; set; }
    [Inject] public IToaster Toastr { get; set; }
    protected object Result { get; set; } = new();
    private readonly JsonSerializerOptions opt = new() { WriteIndented = true };

    protected override async Task OnInitializedAsync()
    {
        await RefreshAsync();
    }

    protected async Task RefreshAsync(bool showToastr = false)
    {
        Result = new { Status = "Please wait" };

        Stopwatch sw = Stopwatch.StartNew();
        var result = await Api.GetAiRuntimeInfoAsync();
        Result = JsonSerializer.Serialize(result, opt);
        sw.Stop();

        if (showToastr)
            Toastr.Success($"Refreshed [{sw.Elapsed}]");
    }
}
