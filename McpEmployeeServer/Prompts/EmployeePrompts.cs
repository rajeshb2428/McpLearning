using ModelContextProtocol.Server;
using System.ComponentModel;

[McpServerPromptType]
public class EmployeePrompts
{
    [McpServerPrompt(
        Name = "employee_summary",
        Title = "Creates a professional summary of an employee.")]
    public string EmployeeSummary(
        [Description("The employee ID.")]
        int employeeId)
    {
        return $"""
                Provide a professional summary for employee {employeeId}.

                Include:
                - Employee name
                - Job title
                - Department
                - Key responsibilities

                Keep the response concise and professional.
                """;
    }
}