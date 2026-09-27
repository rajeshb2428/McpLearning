using ModelContextProtocol.Server;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


var jwtKey = "ThisIsMyMcpLearningSecretKey123456";

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = "McpLearning",
                ValidAudience = "McpEmployeeServer",

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey))
            };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        "EmployeeRead",
        policy =>
        {
            policy.RequireRole("EmployeeReader");
        });
});

builder.Services
    .AddMcpServer()
    .WithHttpTransport()
        .WithTools<EmployeeTools>()    
        .AddAuthorizationFilters();;

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.MapMcp("mcp")
.RequireAuthorization();

app.Run();