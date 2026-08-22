using McpEmployeeServer.Models;
using Microsoft.Data.Sqlite;

namespace McpEmployeeServer.Repositories;

public class EmployeeRepository
{
    private readonly string _connectionString;

    public EmployeeRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public Employee? GetEmployee(int employeeId)
    {
        using var connection = new SqliteConnection(_connectionString);

        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText = """
            SELECT Id, Name, Department, JobTitle
            FROM Employees
            WHERE Id = $id;
            """;

        command.Parameters.AddWithValue("$id", employeeId);

        using var reader = command.ExecuteReader();

        if (!reader.Read())
        {
            return null;
        }

        return new Employee
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            Department = reader.GetString(2),
            JobTitle = reader.GetString(3)
        };
    }
    public List<Employee> GetEmployeesByDepartment(string department)
    {
        var employees = new List<Employee>();

        using var connection = new SqliteConnection(_connectionString);

        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText = """
            SELECT Id, Name, Department, JobTitle
            FROM Employees
            WHERE Department = $department;
            """;

        command.Parameters.AddWithValue("$department", department);

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            employees.Add(new Employee
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Department = reader.GetString(2),
                JobTitle = reader.GetString(3)
            });
        }

        return employees;
    }
}