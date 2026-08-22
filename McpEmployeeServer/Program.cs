// See https://aka.ms/new-console-template for more information
using McpEmployeeServer.Data;
using McpEmployeeServer.Repositories;
using McpEmployeeServer.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// Console.WriteLine("Hello, World!");
var builder = Host.CreateApplicationBuilder(args);
// var connectionString =
//     "Data Source=Data/employees.db";

    var databasePath = Path.Combine(
    AppContext.BaseDirectory,
    "Data",
    "employees.db");

    Directory.CreateDirectory(
    Path.GetDirectoryName(databasePath)!);

var connectionString = $"Data Source={databasePath}";
DatabaseInitializer.Initialize(connectionString);

    builder.Services.AddSingleton(
    new EmployeeRepository(connectionString));

builder.Services.AddSingleton<EmployeeService>();

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    // .WithTools<EmployeeTools>();
    .WithPrompts<EmployeePrompts>()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();