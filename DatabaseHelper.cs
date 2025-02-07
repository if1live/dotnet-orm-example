namespace DemoDatabase;

public enum DatabaseEngine
{
    Sqlite = 1,
    Mysql = 2,
}

public class DatabaseHelper
{
    public static DatabaseEngine SelectEngine(string connectionString)
    {
        return connectionString.StartsWith("Data Source=")
            ? DatabaseEngine.Sqlite
            : DatabaseEngine.Mysql;
    }
}