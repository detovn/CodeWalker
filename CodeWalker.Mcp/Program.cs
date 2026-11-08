using CodeWalker.Mcp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// stdout is the MCP channel. CodeWalker.Core (and anything else) must never write to it,
// so route Console.Out to stderr before the transport takes the raw stdout stream.
Console.SetOut(Console.Error);

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace);
builder.Logging.SetMinimumLevel(LogLevel.Warning);

builder.Services.AddSingleton(CwConfig.Resolve());
builder.Services.AddSingleton<CwGame>();
builder.Services
    .AddMcpServer(o =>
    {
        o.ServerInfo = new() { Name = "codewalker", Version = "1.0.0" };
        o.ServerInstructions = CwInstructions.Text;
    })
    .WithStdioServerTransport()
    .WithToolsFromAssembly()
    .WithRequestFilters(f => f.AddCallToolFilter(next => async (ctx, ct) =>
    {
        // Surface the real reason when CodeWalker.Core throws (the SDK would otherwise
        // replace it with a generic "An error occurred" message).
        try
        {
            return await next(ctx, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException and not ModelContextProtocol.McpException)
        {
            return new ModelContextProtocol.Protocol.CallToolResult
            {
                IsError = true,
                Content = [new ModelContextProtocol.Protocol.TextContentBlock { Text = $"{ex.GetType().Name}: {ex.Message}" }],
            };
        }
    }));

await builder.Build().RunAsync();
