using MCPServers.Shared;
using MCPServers.Shared.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace {{ServerName}}.Services;

public class {{ServerName}}Service : BaseHttpService
{
    private readonly string _resourceUrl;
    private readonly TokenContextAccessor _tokenContextAccessor;
    private readonly TokenExchangeService _tokenExchangeService;

    public {{ServerName}}Service(
        IConfiguration configuration,
        HttpClient client,
        ILogger<{{ServerName}}Service> logger,
        TokenContextAccessor tokenContextAccessor,
        TokenExchangeService tokenExchangeService)
        : base(configuration, client, logger)
    {
        _resourceUrl = configuration["TargetResource:Url"]
            ?? throw new InvalidOperationException("TargetResource:Url is not configured");
        _tokenContextAccessor = tokenContextAccessor;
        _tokenExchangeService = tokenExchangeService;
    }

    public async Task<string> GetDataAsync(string input)
    {
        var tokenContext = _tokenContextAccessor.TokenValidatedContext;
        var accessToken = await _tokenExchangeService.ExchangeTokenAsync(tokenContext, _resourceUrl);

        _client.DefaultRequestHeaders.Remove("Authorization");
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

        var url = $"{_resourceUrl}/TODO-replace-with-endpoint/{Uri.EscapeDataString(input)}";
        return await GetAsync(url);
    }
}
