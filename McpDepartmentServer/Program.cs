using ModelContextProtocol.Server;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithTools<DepartmentTools>();

var app = builder.Build();

app.MapMcp("/mcp");

app.Run();