using ModelContextProtocol.Server;
using System.ComponentModel;

[McpServerToolType]
public class DepartmentTools
{
   [McpServerTool(Name = "get_department")]
    [Description("Gets department details using a department name such as Engineering, Finance, or HR.")]
    public string GetDepartment(string departmentName)
    {
        var normalizedDepartment = departmentName
            .Replace(" Department", "", StringComparison.OrdinalIgnoreCase)
            .Trim();

        if (normalizedDepartment.Equals(
            "Engineering",
            StringComparison.OrdinalIgnoreCase))
        {
            return "Department: Engineering, Manager: John, Location: Austin";
        }

        if (normalizedDepartment.Equals(
            "Finance",
            StringComparison.OrdinalIgnoreCase))
        {
            return "Department: Finance, Manager: David, Location: Dallas";
        }

        return $"Department '{departmentName}' was not found.";
    }
}