using ModelContextProtocol.Server;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddMcpServer()
    .WithHttpTransport()
        .WithTools<EmployeeTools>();;

var app = builder.Build();

app.MapMcp("mcp");

app.Run();