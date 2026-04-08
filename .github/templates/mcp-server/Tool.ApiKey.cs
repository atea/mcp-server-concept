using System.ComponentModel;
using ModelContextProtocol.Server;
using {{ServerName}}.Services;

namespace {{ServerName}};

[McpServerToolType]
public class {{ServerName}}Tool
{
    public static IConfiguration? Configuration { get; set; }
    public static IServiceProvider? ServiceProvider { get; set; }

    [McpServerTool, Description("TODO: Replace with your tool description")]
    public static async Task<string> ExampleTool(
        [Description("TODO: Replace with your parameter description")] string input)
    {
        using var scope = ServiceProvider!.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<{{ServerName}}Service>();
        return await service.GetDataAsync(input);
    }
}
