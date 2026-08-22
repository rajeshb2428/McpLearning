using Microsoft.Data.Sqlite;

namespace McpEmployeeServer.Data;

public static class DatabaseInitializer
{
    public static void Initialize(string connectionString)
    {
        using var connection = new SqliteConnection(connectionString);

        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText = """
            CREATE TABLE IF NOT EXISTS Employees
            (
                Id INTEGER PRIMARY KEY,
                Name TEXT NOT NULL,
                Department TEXT NOT NULL,
                JobTitle TEXT NOT NULL
            );

            INSERT OR IGNORE INTO Employees
                (Id, Name, Department, JobTitle)
            VALUES
                (101, 'Rajesh', 'Engineering', 'Software Engineer');

            INSERT OR IGNORE INTO Employees
                (Id, Name, Department, JobTitle)
            VALUES
                (102, 'John', 'Finance', 'Financial Analyst');

            INSERT OR IGNORE INTO Employees
                (Id, Name, Department, JobTitle)
            VALUES
                (103, 'David', 'HR', 'HR Manager');
            """;

        command.ExecuteNonQuery();
    }
}