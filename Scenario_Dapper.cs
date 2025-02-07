using Dapper;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using DemoDatabase;
using Microsoft.Data.Sqlite;
using MySql.Data.MySqlClient;

namespace Scenario_Dapper;

class Strategy(IDbConnection connection) : IBenchmarkStrategy
{
    public async Task InsertBulkAsync(List<Book> chunk)
    {
        using var transaction = connection.BeginTransaction();
        var sql = "INSERT INTO book(id, lang, title) VALUES (@Id, @Lang, @Title)";
        var rowsAffected = await connection.ExecuteAsync(sql, chunk, transaction);
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

        {
            var sql = "DELETE FROM book";
            var x = await connection.ExecuteAsync(sql);
        }

        {
            var strategy = new Strategy(connection);
            await ScenarioHelper.RunBenchmarkAsync(strategy);
        }

        {
            var sql = "SELECT * FROM book WHERE lang = @Lang";
            var books = await connection.QueryAsync<Book>(sql, new { Lang = "en" });
            var entities = books.ToList();
            Console.WriteLine(entities.Count);
            // foreach (var ent in entities)
            // {
            //     Console.WriteLine($"id={ent.Id}, lang={ent.Lang}");
            // }
        }
    }
}