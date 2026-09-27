using OpenAI;
using OpenAI.Chat;

using ModelContextProtocol.Client;
using System.Text.Json;

var apiKey = "AP-Key";
//Environment.GetEnvironmentVariable("OPENAI_API_KEY");

if (string.IsNullOrWhiteSpace(apiKey))
{
    Console.WriteLine("OPENAI_API_KEY is not set.");
    return;
}

var token = McpAuthHelper.CreateDevelopmentToken();

var httpClient = new HttpClient();

httpClient.DefaultRequestHeaders.Authorization =
    new System.Net.Http.Headers.AuthenticationHeaderValue(
        "Bearer",
        token);

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
        Endpoint = new Uri("http://localhost:5031/mcp"),
          TransportMode = HttpTransportMode.StreamableHttp,

        AdditionalHeaders = new Dictionary<string, string>
        {
            ["Authorization"] = $"Bearer {token}"
        }
    });

var departmentTransport = new HttpClientTransport(
    new HttpClientTransportOptions
    {
        Endpoint = new Uri("http://localhost:5119/mcp"),
                TransportMode = HttpTransportMode.StreamableHttp
    });

await using var employeeMcpClient =
    await McpClient.CreateAsync(employeeTransport);

await using var departmentMcpClient =
    await McpClient.CreateAsync(departmentTransport);

//     var employeeTools =
//     await employeeMcpClient.ListToolsAsync();

// var departmentTools =
//     await departmentMcpClient.ListToolsAsync();


// new logic starts here

var toolToClient =
    new Dictionary<string, McpClient>();

var toolRegistry =
    new Dictionary<string, (McpClient Client, McpClientTool Tool)>();

var chatOptions =
    new ChatCompletionOptions();

async Task RefreshToolsAsync()
{
    Console.WriteLine("\nRefreshing MCP capabilities...");

    // 1. Discover current tools from both MCP servers
    var employeeTools =
        await employeeMcpClient.ListToolsAsync();

    var departmentTools =
        await departmentMcpClient.ListToolsAsync();

    // 2. Clear the old registry
    toolToClient.Clear();
    toolRegistry.Clear();   

    // 3. Rebuild the MCP tool → MCP client mapping
    foreach (var tool in employeeTools)
    {
        if (!toolRegistry.TryAdd(
            tool.Name,
            (employeeMcpClient, tool)))
    {
        throw new InvalidOperationException(
            $"Duplicate MCP tool name detected: {tool.Name}");
    }
    }

    foreach (var tool in departmentTools)
    {
        if (!toolRegistry.TryAdd(
            tool.Name,
            (departmentMcpClient, tool)))
    {
        throw new InvalidOperationException(
            $"Duplicate MCP tool name detected: {tool.Name}");
    }
    }

    // 4. Rebuild the tools exposed to OpenAI
    chatOptions.Tools.Clear();

    var allMcpTools =
        employeeTools
            .Concat(departmentTools)
            .ToList();

    foreach (var tool in allMcpTools)
    {
        var openAiTool =
            ChatTool.CreateFunctionTool(
                functionName: tool.Name,
                functionDescription: tool.Description,
                functionParameters:
                    BinaryData.FromString(
                        tool.JsonSchema.ToString()));

        chatOptions.Tools.Add(openAiTool);
    }

    // 5. Display current capabilities
    Console.WriteLine("\nEmployee MCP Tools:");

    foreach (var tool in employeeTools)
    {
        Console.WriteLine($"- {tool.Name}");
    }

    Console.WriteLine("\nDepartment MCP Tools:");

    foreach (var tool in departmentTools)
    {
        Console.WriteLine($"- {tool.Name}");
    }

    Console.WriteLine(
        $"\nTotal discovered MCP tools: {allMcpTools.Count}");
}

await RefreshToolsAsync();

static bool RequiresApproval(McpClientTool tool)
{
    var annotations = tool.ProtocolTool.Annotations;

    // Conservative policy:
    // anything that is not explicitly read-only requires approval.
    if (annotations?.ReadOnlyHint != true)
    {
        return true;
    }

    if (annotations.DestructiveHint == true)
    {
        return true;
    }

    return false;
}
// new login ends here

// var messages1 = new List<ChatMessage>
// {
//     new UserChatMessage(
//         "Get employee 101 and provide details about that employee's department. And who is his manager?")
// };
var messages1 = new List<ChatMessage>
{
    new UserChatMessage(
        "Delete employee 101")
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

        if (!toolRegistry.TryGetValue(
        toolCall.FunctionName,
        out var registration))
{
    throw new InvalidOperationException(
        $"No MCP tool registered for tool " +
        $"'{toolCall.FunctionName}'.");
}
        var targetClient = registration.Client;
var tool = registration.Tool;

        if (RequiresApproval(tool))
        {
            Console.WriteLine();
            Console.WriteLine(
                $"⚠️ Approval required for: {tool.Name}");

            Console.WriteLine(
                $"Description: {tool.Description}");

            var annotations = tool.ProtocolTool.Annotations;

            Console.WriteLine(
                $"ReadOnly: {annotations?.ReadOnlyHint}");

            Console.WriteLine(
                $"Destructive: {annotations?.DestructiveHint}");

            Console.Write("Do you want to execute this tool? (y/n): ");

            var answer = Console.ReadLine();

            if (!string.Equals(
                    answer,
                    "y",
                    StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Tool execution cancelled.");
                continue;
            }
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

