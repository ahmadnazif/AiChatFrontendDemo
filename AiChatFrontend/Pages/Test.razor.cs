using AiChatFrontend.Services;
using Microsoft.AspNetCore.Components;

namespace AiChatFrontend.Pages;

public class TestBase : ComponentBase
{
    [Inject] public ApiClient Api { get; set; }
    [Inject] public ILogger<TestBase> Logger { get; set; }
    protected string StreamGet { get; set; }
    protected string StreamPost { get; set; }

    protected async Task StreamGetAsync()
    {
        //await foreach(var item in Api.StreamGetAsync(30))
        //{
        //    StreamGet += item;
        //    Logger.LogInformation($"GET: {item}");
        //    StateHasChanged();
        //}
    }

    protected async Task StreamPostAsync()
    {
        //await foreach (var item in Api.StreamPostAsync(30))
        //{
        //    StreamPost += item;
        //    Logger.LogInformation($"POST: {item}");
        //    StateHasChanged();
        //}
    }
}
