using System.Net.Http.Json;
using System.Runtime.CompilerServices;

namespace AiChatFrontend.Extensions;

public static class HttpClientExtension
{
    public static async IAsyncEnumerable<T> PostAsAsyncEnumerable<T>(this HttpClient httpClient, string url, object body, [EnumeratorCancellation] CancellationToken ct)
    {
        HttpRequestMessage req = new()
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(url),
            Content = JsonContent.Create(body)
        };

        var response = await httpClient.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);

        response.EnsureSuccessStatusCode();
        var stream = await response.Content.ReadAsStreamAsync(ct);
        var items = JsonSerializer.DeserializeAsyncEnumerable<T>(stream, cancellationToken: ct);

        if (items is not null)
        {
            await foreach (var item in items)
            {
                if (item is not null)
                    yield return item;
            }
        }
    }
}
