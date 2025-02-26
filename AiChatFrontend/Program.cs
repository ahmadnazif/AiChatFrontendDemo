global using System.Text.Json;
global using AiChatFrontend.Models;
global using AiChatFrontend.Enums;
using AiChatFrontend;
using AiChatFrontend.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Sotsera.Blazor.Toaster.Core.Models;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
var config = builder.Configuration;

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSingleton<CacheService>();
builder.Services.AddScoped<ChatService>();

builder.Services.AddToaster(new ToasterConfiguration
{
    PositionClass = Defaults.Classes.Position.TopCenter,
    PreventDuplicates = true,
    NewestOnTop = true,
    ShowCloseIcon = true,
    ShowProgressBar = true,
    VisibleStateDuration = 5000,
    MaximumOpacity = 100,
    ShowTransitionDuration = 100,
    HideTransitionDuration = 100,
    MaxDisplayedToasts = 3
});

builder.Services.AddHttpClient<ApiClient>((sp, client) =>
{
    client.BaseAddress = new Uri(config["ApiUrl"]);
});

await builder.Build().RunAsync();
