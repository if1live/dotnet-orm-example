using System.Data;
using DemoDatabase;
using Microsoft.Data.Sqlite;
using MySql.Data.MySqlClient;
using SqlKata.Compilers;
using SqlKata.Execution;

namespace Scenario_QueryBuilder;

class Strategy(QueryFactory db) : IBenchmarkStrategy
{
    public async Task InsertBulkAsync(List<Book> chunk)
    {
        var columns = new[] { "id", "lang", "title" };
        var list = chunk.Select(x =>
        {
            return new object[]
            {
                x.Id,
                x.Lang,
                x.Title,
            };
        });

        using var transaction = db.Connection.BeginTransaction();
        var insertQuery = db.Query("book").AsInsert(columns, list);
        await db.ExecuteAsync(insertQuery, transaction);
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

        Compiler compiler = engine == DatabaseEngine.Sqlite
            ? new SqliteCompiler()
            : new MySqlCompiler();
        var db = new QueryFactory(connection, compiler);
        // {
        //     Logger = compiled => Console.WriteLine($"query: {compiled.ToString()}")
        // };

        await db.Query("book").DeleteAsync();

        // 문제: insert many가 멀쩡하지 않음!
        var strategy = new Strategy(db);
        await ScenarioHelper.RunBenchmarkAsync(strategy);

        var founds = await db.Query("book").Where("lang", "en").GetAsync<Book>();
        var entities = founds.ToList();
        Console.WriteLine(entities.Count);
        /*
        foreach (var ent in entities)
        {
            var hexString = BitConverter.ToString(ent.Payload).Replace("-", " ");
            Console.WriteLine($"id={ent.Id}, lang={ent.Lang} payload={hexString}");
        }
        */
    }
}