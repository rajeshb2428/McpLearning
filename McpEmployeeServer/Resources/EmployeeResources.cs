using ModelContextProtocol.Server;
using System.ComponentModel;

[McpServerResourceType]
public class EmployeeResources
{
    [McpServerResource(
        UriTemplate = "employee://{employeeId}",
        Name = "Employee",
        // Description = "Provides employee information.",
        MimeType = "text/plain")]
    public string GetEmployeeResource(
        string employeeId)
    {
        if (employeeId == "101")
        {
            return """
                   Employee ID: 101
                   Name: Rajesh
                   Title: Software Engineer
                   Department: Engineering
                   """;
        }

        return $"Employee {employeeId} was not found.";
    }
}