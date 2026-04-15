using System.ComponentModel;
using ModelContextProtocol.Server;
using FBIMostWanted.Services;

namespace FBIMostWanted;

[McpServerToolType]
public class FBIMostWantedTool
{
    private readonly FBIMostWantedService _service;

    public FBIMostWantedTool(FBIMostWantedService service)
    {
        _service = service;
    }

    [McpServerTool, Description("List FBI Most Wanted persons. Optionally filter by field office and paginate results.")]
    public async Task<string> ListWanted(
        [Description("FBI field office to filter by (e.g. 'miami', 'new_york'). Leave empty for all offices.")] string? fieldOffice = null,
        [Description("Page number for paginated results. Defaults to 1.")] int page = 1)
    {
        return await _service.ListWantedAsync(fieldOffice, page);
    }
}
