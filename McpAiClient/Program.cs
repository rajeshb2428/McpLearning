using OpenAI;
using OpenAI.Chat;

using ModelContextProtocol.Client;
using System.Text.Json;

var apiKey = "";
//Environment.GetEnvironmentVariable("OPENAI_API_KEY");

if (string.IsNullOrWhiteSpace(apiKey))
{
    Console.WriteLine("OPENAI_API_KEY is not set.");
    return;
}


var client = new ChatClient(
    model: "gpt-4o-mini",
    apiKey: apiKey);

var serverProjectPath =
    Path.GetFullPath(
        Path.Combine(
            AppContext.BaseDirectory,
            "../../../../McpEmployeeServer"));

// It is STDIO
var clientTransport = new StdioClientTransport(
    new StdioClientTransportOptions
    {
        Name = "Employee MCP Server",
        Command = "dotnet",
        Arguments =
        [
            "run",
            "--project",
            serverProjectPath
        ]
    });


// It is HTTP request
// var clientTransport = new HttpClientTransport(
//     new HttpClientTransportOptions
//     {
//         Endpoint = new Uri("http://localhost:5031/mcp")
//     });

var employeeTransport = new HttpClientTransport(
    new HttpClientTransportOptions
    {
        Endpoint = new Uri("http://localhost:5031/mcp")
    });

var departmentTransport = new HttpClientTransport(
    new HttpClientTransportOptions
    {
        Endpoint = new Uri("http://localhost:5119/mcp")
    });

await using var employeeMcpClient =
    await McpClient.CreateAsync(employeeTransport);

await using var departmentMcpClient =
    await McpClient.CreateAsync(departmentTransport);

    var employeeTools =
    await employeeMcpClient.ListToolsAsync();

var departmentTools =
    await departmentMcpClient.ListToolsAsync();
Console.WriteLine("Employee MCP Tools:");

foreach (var tool in employeeTools)
{
    Console.WriteLine($"- {tool.Name}");
}

Console.WriteLine("\nDepartment MCP Tools:");

foreach (var tool in departmentTools)
{
    Console.WriteLine($"- {tool.Name}");
}

var toolToClient =
    new Dictionary<string, McpClient>();

foreach (var tool in employeeTools)
{
    if (!toolToClient.TryAdd(tool.Name, employeeMcpClient))
    {
        throw new InvalidOperationException(
            $"Duplicate MCP tool name detected: {tool.Name}");
    }
}

foreach (var tool in departmentTools)
{
    if (!toolToClient.TryAdd(tool.Name, departmentMcpClient))
    {
        throw new InvalidOperationException(
            $"Duplicate MCP tool name detected: {tool.Name}");
    }
}

var allMcpTools =
    employeeTools
        .Concat(departmentTools)
        .ToList();


var openAiTools = new List<ChatTool>();

foreach (var tool in allMcpTools)
{
    var openAiTool =
        ChatTool.CreateFunctionTool(
            functionName: tool.Name,
            functionDescription: tool.Description,
            functionParameters:
                BinaryData.FromString(
                    tool.JsonSchema.ToString()));

    openAiTools.Add(openAiTool);
}



var chatOptions =
    new ChatCompletionOptions();

foreach (var tool in openAiTools)
{
    chatOptions.Tools.Add(tool);
}

var messages1 = new List<ChatMessage>
{
    new UserChatMessage(
        "Get employee 101 and provide details about that employee's department. And who is his manager?")
};
while (true)
{
    var response = await client.CompleteChatAsync(
        messages1,
        chatOptions);

    Console.WriteLine($"Total Tools count: {response.Value.ToolCalls.Count.ToString()}");
    if (response.Value.FinishReason != ChatFinishReason.ToolCalls)
    {
         Console.WriteLine("\nFinal Answer:");

        foreach (var content in response.Value.Content)
        {
            Console.WriteLine(content.Text);
        }
        // Final answer
        break;
    }
    messages1.Add(new AssistantChatMessage(response.Value));

    foreach (var toolCall in response.Value.ToolCalls)
    {
        Console.WriteLine("--------------------------------");
        Console.WriteLine($"Tool requested: {toolCall.FunctionName}");
        Console.WriteLine($"Arguments: {toolCall.FunctionArguments}");

        var arguments =
            JsonSerializer.Deserialize<Dictionary<string, object?>>(
                toolCall.FunctionArguments.ToString());

        if (!toolToClient.TryGetValue(
                toolCall.FunctionName,
                out var targetClient))
        {
            throw new InvalidOperationException(
                $"No MCP server registered for tool " +
                $"'{toolCall.FunctionName}'.");
        }

        var toolResult =
            await targetClient.CallToolAsync(
                toolCall.FunctionName,
                arguments);

        var resultText = string.Join(
        Environment.NewLine,
        toolResult.Content.Select(c => c.ToString()));

        Console.WriteLine("MCP Tool Result:");
            Console.WriteLine(resultText);

        messages1.Add(
        new ToolChatMessage(
            toolCall.Id,
            resultText));
    }
}
