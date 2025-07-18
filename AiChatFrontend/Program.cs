global using System.Text.Json;
global using AiChatFrontend.Models;
global using AiChatFrontend.Enums;
global using AiChatFrontend.Helpers;
global using AiChatFrontend.Events;
global using AiChatFrontend.Extensions;
global using AiChatFrontend.Shared;
using AiChatFrontend;
using AiChatFrontend.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Sotsera.Blazor.Toaster.Core.Models;
using Toolbelt.Blazor.Extensions.DependencyInjection;
using AiChatFrontend.CacheServices;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
var config = builder.Configuration;

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSingleton<SessionCache>();
builder.Services.AddScoped<SessionService>();
builder.Services.AddSingleton<ChatCache>();
builder.Services.AddSingleton<FileChatCache>();
builder.Services.AddScoped<ChatServiceOld>();
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

//builder.Services.AddHttpClient<ApiClient>((sp, client) =>
//{
//    client.BaseAddress = new Uri(config["ApiUrl"]);
//});

builder.Services.AddLoadingBarService();

builder.Services.AddHttpClient(nameof(ApiClient), (sp, client) =>
{
    client.BaseAddress = new Uri(config["ApiUrl"]);
    client.EnableIntercept(sp);
});
builder.Services.AddScoped<ApiClient>();

builder.UseLoadingBar();
await builder.Build().RunAsync();
