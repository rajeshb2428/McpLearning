using McpEmployeeServer.Services;
using ModelContextProtocol.Server;
using System.ComponentModel;

[McpServerToolType]
public class EmployeeTools
{
    private readonly EmployeeService _employeeService;

    public EmployeeTools(EmployeeService employeeService)
    {
        _employeeService = employeeService;
    }
    
    [McpServerTool]
    // [Description("Gets employee information using the employee ID.")]
    [Description("Use this tool when you need details about exactly one employee. Provide the employee ID.")]
    public string GetEmployee(int employeeId)
    {
        var employee = _employeeService.GetEmployee(employeeId);

        if (employee == null)
        {
            return $"Employee {employeeId} was not found.";
        }

        return
            $"Employee {employee.Id}: " +
            $"{employee.Name}, " +
            $"{employee.JobTitle}, " +
            $"{employee.Department} Department";
    }

    [McpServerTool(Name = "get_employees_by_department")]
    // [Description("Gets all employees belonging to a department.")]
    [Description("Use this tool when you need a list of employees belonging to a specific department.")]
    public string GetEmployeesByDepartment(string department)
    {
        var employees =
            _employeeService.GetEmployeesByDepartment(department);

        if (employees.Count == 0)
        {
            return $"No employees found in department: {department}";
        }

        return string.Join(
            Environment.NewLine,
            employees.Select(e =>
                $"{e.Id}: {e.Name}, {e.JobTitle}"));
    }

    [McpServerTool]
    [Description("Gets employee information using the employee ID.")]
    public string Gettestme(int employeeId)
    {
        if (employeeId == 101)
        {
            return "Employee 101: Rajesh, Software Engineer, Engineering Department";
        }

        return $"Employee {employeeId} was not found.";
    }
}