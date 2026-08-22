using McpEmployeeServer.Models;
using McpEmployeeServer.Repositories;
using Microsoft.Data.Sqlite;

namespace McpEmployeeServer.Services;

public class EmployeeService
{
    private readonly EmployeeRepository _repository;

    public EmployeeService(EmployeeRepository repository)
    {
        _repository = repository;
    }

    public Employee? GetEmployee(int employeeId)
    {
        return _repository.GetEmployee(employeeId);
    }
    public List<Employee> GetEmployeesByDepartment(string department)
    {
        return _repository.GetEmployeesByDepartment(department);
    }
}