using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

public class McpAuthHelper
{
    public static string CreateDevelopmentToken()
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                "ThisIsMyMcpLearningSecretKey123456"));

        var credentials =
            new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(
                ClaimTypes.Name,
                "McpLearningClient"),

            new Claim(
                ClaimTypes.Role,
                "EmployeeReader")
        };

        var token = new JwtSecurityToken(
            issuer: "McpLearning",
            audience: "McpEmployeeServer",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }
}