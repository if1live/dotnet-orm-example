using Dapper;
using System.ComponentModel.DataAnnotations.Schema;
using DemoDatabase;
using Microsoft.Data.Sqlite;

namespace Scenario_Dapper;

[Table("book")]
class Book
{
    public int Id { get; set; }
    public string Lang { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
}

public class Executor : IScenario
{
    public async Task ExecuteAsync()
    {
        var connection = new SqliteConnection("Data Source=hello.db");

        {
            var sql = "DELETE FROM book";
            var x = await connection.ExecuteAsync(sql);
        }

        {
            var sql = "INSERT INTO book(id, lang, title) VALUES (@Id, @Lang, @Title)";

            var b1 = new Book() { Id = 1, Lang = "en", Title = "foo" };
            var b2 = new Book() { Id = 2, Lang = "en", Title = "bar" };
            var books = new List<Book> { b1, b2 };

            var rowsAffected = await connection.ExecuteAsync(sql, books);
            Console.WriteLine($"{rowsAffected} row(s) inserted.");
        }

        {
            var sql = "SELECT * FROM book WHERE lang = @Lang";
            var books = await connection.QueryAsync<Book>(sql, new { Lang = "en" });
            var entities = books.ToList();
            Console.WriteLine(entities.Count);
            foreach (var ent in entities)
            {
                Console.WriteLine($"id={ent.Id}, lang={ent.Lang}");
            }
        }
    }
}