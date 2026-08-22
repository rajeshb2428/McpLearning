using ModelContextProtocol.Server;
using System.ComponentModel;

[McpServerToolType]
public class EmployeeTools
{
    [McpServerTool]
    [Description("Gets employee information using the employee ID.")]
    public string GetEmployee(int employeeId)
    {
        if (employeeId == 101)
        {
            return "Employee 101: Rajesh, Software Engineer, Engineering Department";
        }

        return $"Employee {employeeId} was not found.";
    }
}