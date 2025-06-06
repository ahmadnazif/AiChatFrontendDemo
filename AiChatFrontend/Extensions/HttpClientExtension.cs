using Microsoft.AspNetCore.Components.WebAssembly.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;

namespace AiChatFrontend.Extensions;

public static class HttpClientExtension
{
    private static readonly JsonSerializerOptions jso = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static async IAsyncEnumerable<T> PostAsAsyncEnumerable<T>(this HttpClient httpClient, string url, object body, [EnumeratorCancellation] CancellationToken ct)
    {
        HttpRequestMessage request = new(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(body)
        };

        request.SetBrowserResponseStreamingEnabled(true);

        var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);

        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(ct);

        await foreach (var item in JsonSerializer.DeserializeAsyncEnumerable<T>(stream, jso, ct))
        {
            if (item is not null)
                yield return item;
        }
    }
}
