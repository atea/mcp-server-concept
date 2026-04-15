using System.Text;
using System.Text.Json;
using MCPServers.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FBIMostWanted.Services;

public class FBIMostWantedService : BaseHttpService
{
    private readonly string _baseUrl;

    public FBIMostWantedService(
        IConfiguration configuration,
        HttpClient client,
        ILogger<FBIMostWantedService> logger)
        : base(configuration, client, logger)
    {
        _baseUrl = (configuration["FBIMostWantedApi:BaseUrl"] ?? "https://api.fbi.gov").TrimEnd('/');
    }

    public async Task<string> ListWantedAsync(string? fieldOffice = null, int page = 1)
    {
        var query = new StringBuilder($"{_baseUrl}/wanted/v1/list?page={page}");
        if (!string.IsNullOrWhiteSpace(fieldOffice))
            query.Append($"&field_offices={Uri.EscapeDataString(fieldOffice.ToLowerInvariant())}");

        var json = await GetAsync(query.ToString());

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var total = root.GetProperty("total").GetInt32();
        var items = root.GetProperty("items");

        var sb = new StringBuilder();
        sb.AppendLine($"Total results: {total} | Page: {page}");
        sb.AppendLine();

        foreach (var item in items.EnumerateArray())
        {
            var title = item.TryGetProperty("title", out var t) ? t.GetString() : "Unknown";
            var status = item.TryGetProperty("status", out var s) ? s.GetString() : null;
            var reward = item.TryGetProperty("reward_text", out var r) ? r.GetString() : null;
            var url = item.TryGetProperty("url", out var u) ? u.GetString() : null;

            sb.AppendLine($"- {title}");
            if (!string.IsNullOrWhiteSpace(status))
                sb.AppendLine($"  Status: {status}");
            if (!string.IsNullOrWhiteSpace(reward))
                sb.AppendLine($"  Reward: {reward}");
            if (!string.IsNullOrWhiteSpace(url))
                sb.AppendLine($"  URL: {url}");
        }

        return sb.ToString();
    }
}
