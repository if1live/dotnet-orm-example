using System.Data;
using DemoDatabase;
using Dommel;
using Microsoft.Data.Sqlite;
using MySql.Data.MySqlClient;

namespace Scenario_Dommel;

class Strategy(IDbConnection connection) : IBenchmarkStrategy
{
    public async Task InsertBulkAsync(List<Book> chunk)
    {
        using var transaction = connection.BeginTransaction();
        await connection.InsertAllAsync(chunk, transaction);
        transaction.Commit();
    }
}

public class Executor : IScenario
{
    public async Task ExecuteAsync(string connectionString)
    {
        var engine = DatabaseHelper.SelectEngine(connectionString);
        using IDbConnection connection = engine == DatabaseEngine.Sqlite
            ? new SqliteConnection(connectionString)
            : new MySqlConnection(connectionString);
        connection.Open();

        /*
        DommelMapper.LogReceived = sql =>
        {
            var isDelete = sql.Contains(": delete");
            var isInsert = sql.Contains(": insert");
            var isSelect = sql.Contains(": select");
            if (isDelete || isInsert || isSelect)
                Console.WriteLine($"query: {sql}");
        };
        */

        var x = connection.DeleteAllAsync<Book>();

        var strategy = new Strategy(connection);
        await ScenarioHelper.RunBenchmarkAsync(strategy);

        // ID로 GUID를 사용하면 select에서 문제 발생
        // Unhandled exception. System.Data.DataException: Error parsing column 0 (Id=3F4E0F33-22C2-02E5-F8DB-70C4B2FE30BA - String)
        //     ---> System.InvalidCastException: Invalid cast from 'System.String' to 'System.Guid'
        // dommel 문서볼때 지원하는 기능이긴한데 추가 정의가 필요한듯
        var q = await connection.SelectAsync<Book>(x => x.Lang == "en");
        var entities = q.ToList();
        Console.WriteLine(entities.Count);
    }
}