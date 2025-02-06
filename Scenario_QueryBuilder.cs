using DemoDatabase;
using Microsoft.Data.Sqlite;
using SqlKata.Compilers;
using SqlKata.Execution;

namespace Scenario_QueryBuilder;

class Book
{
    public int? Id { get; set; }
    public string Lang { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public byte[] Payload { get; set; } = [];
}

public class Executor : IScenario
{
    public async Task ExecuteAsync()
    {
        var connection = new SqliteConnection("Data Source=hello.db");
        var compiler = new SqliteCompiler();
        var db = new QueryFactory(connection, compiler)
        {
            Logger = compiled => Console.WriteLine($"query: {compiled.ToString()}")
        };

        await db.Query("book").DeleteAsync();

        var b1 = new Book() { Lang = "en", Title = "foo", Payload = [1, 2] };
        await db.Query("book").InsertAsync(b1);

        // 문제: insert many가 멀쩡하지 않음!
        var columns = new[] { "lang", "title", "payload" };
        var b2 = new object[] { "en", "bar", new byte[] { 3, 4 } };
        var b3 = new object[] { "en", "span", new byte[] { 0xab, 0xcd } };
        var insertQuery = db.Query("book").AsInsert(columns, [b2, b3]);
        await db.ExecuteAsync(insertQuery);

        var founds = await db.Query("book").Where("lang", "en").GetAsync<Book>();
        var entities = founds.ToList();

        Console.WriteLine(entities.Count);
        foreach (var ent in entities)
        {
            var hexString = BitConverter.ToString(ent.Payload).Replace("-", " ");
            Console.WriteLine($"id={ent.Id}, lang={ent.Lang} payload={hexString}");
        }
    }
}