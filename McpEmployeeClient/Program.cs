using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;


var builder = Host.CreateApplicationBuilder(args);

// var clientTransport = new StdioClientTransport(
//     new StdioClientTransportOptions
//     {
//         Name = "Employee MCP Server",
//         Command = "dotnet",
//         Arguments =
//         [
//             "run",
//             "--project",
//             "../McpEmployeeServer/McpEmployeeServer.csproj"
//         ]
//     });

    // It is HTTP request
var clientTransport = new HttpClientTransport(
    new HttpClientTransportOptions
    {
        Endpoint = new Uri("http://localhost:5031/mcp")
    });

await using var mcpClient = await McpClient.CreateAsync(clientTransport);

Console.WriteLine("Connected to MCP server.");

var tools = await mcpClient.ListToolsAsync();

Console.WriteLine("\nAvailable MCP tools:");

foreach (var tool in tools)
{
    Console.WriteLine("----------------------------------------");
    Console.WriteLine($"Name: {tool.Name}");
    Console.WriteLine($"Description: {tool.Description}");

    Console.WriteLine("Input Schema:");
    Console.WriteLine(tool.JsonSchema);
}

var result = await mcpClient.CallToolAsync(
    "get_employee",
    new Dictionary<string, object?>
    {
        ["employeeId"] = 101
    });

    Console.WriteLine("\nTool result:");

foreach (var content in result.Content)
{
    Console.WriteLine(content);
}

var departmentResult = await mcpClient.CallToolAsync(
    "get_employees_by_department",
    new Dictionary<string, object?>
    {
        ["department"] = "Engineering"
    });

Console.WriteLine("\nDepartment result:");

foreach (var content in departmentResult.Content)
{
    Console.WriteLine(content);
}