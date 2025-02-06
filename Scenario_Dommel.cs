using System.ComponentModel.DataAnnotations.Schema;
using DemoDatabase;
using Dommel;
using Microsoft.Data.Sqlite;

namespace Scenario_Dommel;

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
        DommelMapper.LogReceived = sql =>
        {
            var isDelete = sql.Contains(": delete");
            var isInsert = sql.Contains(": insert");
            var isSelect = sql.Contains(": select");
            if (isDelete || isInsert || isSelect)
                Console.WriteLine($"query: {sql}");
        };

        var x = connection.DeleteAllAsync<Book>();

        var b1 = new Book() { Id = 1, Lang = "en", Title = "foo" };
        var b2 = new Book() { Id = 2, Lang = "en", Title = "bar" };
        var b3 = new Book() { Id = 3, Lang = "en", Title = "spam" };
        var books = new List<Book> { b1, b2, b3 };

        await connection.InsertAllAsync(books);

        // Read
        var q = await connection.SelectAsync<Book>(x => x.Lang == "en");
        var entities = q.ToList();
        Console.WriteLine(entities.Count);
        foreach (var ent in entities)
        {
            Console.WriteLine($"id={ent.Id}, lang={ent.Lang}");
        }
    }
}