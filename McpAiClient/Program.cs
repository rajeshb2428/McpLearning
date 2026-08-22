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

var messages = new List<ChatMessage>
{
    new UserChatMessage("Explain MCP in one sentence.")
};

// var response = await client.CompleteChatAsync(messages);

// Console.WriteLine("LLM Response:");
// Console.WriteLine(response.Value.Content[0].Text);


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

await using var mcpClient =
    await McpClient.CreateAsync(clientTransport);

Console.WriteLine("Connected to MCP server.");

var prompts = await mcpClient.ListPromptsAsync();

Console.WriteLine("\nAvailable MCP prompts:");

foreach (var prmp in prompts)
{
    Console.WriteLine("--------------------------------");
    Console.WriteLine($"Name: {prmp.Name}");
    Console.WriteLine($"Description: {prmp.Description}");
}
var prompt = await mcpClient.GetPromptAsync(
    "employee_summary",
    new Dictionary<string, object?>
    {
        ["employeeId"] = 101
    });

    Console.WriteLine("\nPrompt result:");

foreach (var message in prompt.Messages)
{
    Console.WriteLine("--------------------------------");
    Console.WriteLine(message.Content);
}


var tools = await mcpClient.ListToolsAsync();

Console.WriteLine("\nAvailable MCP tools:");
var openAiTools = new List<ChatTool>();
foreach (var tool in tools)
{
    var openAiTool = ChatTool.CreateFunctionTool(
        functionName: tool.Name,
        functionDescription: tool.Description,
        functionParameters: BinaryData.FromString(
            tool.JsonSchema.ToString()));

    openAiTools.Add(openAiTool);
}
var chatOptions = new ChatCompletionOptions();

foreach (var tool in openAiTools)
{
    chatOptions.Tools.Add(tool);
}
// var messages1 = new List<ChatMessage>
// {
//     new UserChatMessage(
//         "Give me information about employee 101 and tell me how many employees are in the Engineering department?")
// };

var messages1 = new List<ChatMessage>
{
    new UserChatMessage(
        prompt.Messages[0].Content.ToString())
};
while (true)
{
    var response = await client.CompleteChatAsync(
        messages1,
        chatOptions);

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
    // foreach (var content in response.Value.Content)
    // {
    //     Console.WriteLine($"Content type: {content.GetType().Name}");
    // }

    foreach (var toolCall in response.Value.ToolCalls)
    {
        Console.WriteLine("--------------------------------");
        Console.WriteLine($"Tool requested: {toolCall.FunctionName}");
        Console.WriteLine($"Arguments: {toolCall.FunctionArguments}");

        var arguments =
            JsonSerializer.Deserialize<Dictionary<string, object?>>(
                toolCall.FunctionArguments.ToString());

        var toolResult = await mcpClient.CallToolAsync(
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

    // var finalResponse = await client.CompleteChatAsync(
    // messages1,
    // chatOptions);

    // Console.WriteLine("\nFinal Answer:");
    // Console.WriteLine(
    // finalResponse.Value.Content[0].Text);

// foreach (var tool in tools)
// {
//     Console.WriteLine("--------------------------------");
//     Console.WriteLine($"Name: {tool.Name}");
//     Console.WriteLine($"Description: {tool.Description}");
//         Console.WriteLine($"Schema: {tool.JsonSchema}");

// }