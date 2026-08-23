using ModelContextProtocol.Server;
using System.ComponentModel;

[McpServerToolType]
public class EmployeeTools
{
    [McpServerTool]
    [Description("Gets employee information using the employee ID.")]
    public object? GetEmployee(int employeeId)
    {
        if (employeeId == 101)
        {
            return new
            {
                employeeId = 101,
                name = "Rajesh",
                jobTitle = "Software Engineer",
                department = "Engineering"
            };
        }

    return null;
    }

    [McpServerTool(Name = "get_employees_by_department")]
    // [Description("Gets all employees belonging to a department.")]
    [Description("Use this tool when you need a list of employees belonging to a specific department.")]
    public string GetEmployeesByDepartment(string department)
    {
        // var employees =
        //     _employeeService.GetEmployeesByDepartment(department);

        // if (employees.Count == 0)
        // {
        //     return $"No employees found in department: {department}";
        // }

        // return string.Join(
        //     Environment.NewLine,
        //     employees.Select(e =>
        //         $"{e.Id}: {e.Name}, {e.JobTitle}"));
        return $"Employee 101 in departmetn {department}";
    }
}