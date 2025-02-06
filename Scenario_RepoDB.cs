using System.ComponentModel.DataAnnotations.Schema;
using DemoDatabase;
using Microsoft.Data.Sqlite;
using RepoDb;
using RepoDb.Interfaces;

namespace Scenario_RepoDB;

[Table("book")]
public class Book
{
    public int Id { get; set; }
    public string Lang { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
}

public class MyCustomTrace : ITrace
{
    public void BeforeExecution(CancellableTraceLog log)
    {
        Console.WriteLine($"BeforeExecution: {log.Statement}");
    }

    public void AfterExecution<TResult>(ResultTraceLog<TResult> log)
    {
        // Console.WriteLine($"AfterExecution: {log.Result}, TotalTime: {log.ExecutionTime.TotalSeconds} second(s)");
    }

    public Task BeforeExecutionAsync(CancellableTraceLog log,
        CancellationToken cancellationToken = new CancellationToken())
    {
        Console.WriteLine($"BeforeExecutionAsync: {log.Statement}");
        return Task.CompletedTask;
    }

    public Task AfterExecutionAsync<TResult>(ResultTraceLog<TResult> log,
        CancellationToken cancellationToken = new CancellationToken())
    {
        // Console.WriteLine($"AfterExecutionAsync: {log.Result}, TotalTime: {log.ExecutionTime.TotalSeconds} second(s)");
        return Task.CompletedTask;
    }
}

public class Executor : IScenario
{
    public async Task ExecuteAsync()
    {
        GlobalConfiguration
            .Setup()
            .UseSqlite();

        var connection = new SqliteConnection("Data Source=hello.db");

        await connection.DeleteAllAsync<Book>(trace: new MyCustomTrace());

        // 문제: n개의 insert + select의 반복된 쿼리 생성. 레코드 정보 전체를 제공하니까 select 필요 없고, insert 하나로 다 넣고 싶은데
        // BulkAsync를 RepoDB에서 지원하지만 mssql만 된다고 문서에 써있음
        var b1 = new Book() { Id = 1, Lang = "en", Title = "foo" };
        var b2 = new Book() { Id = 2, Lang = "en", Title = "bar" };
        var b3 = new Book() { Id = 3, Lang = "en", Title = "spam" };
        var books = new List<Book> { b1, b2, b3 };
        var fields = Field.Parse<Book>(e => new
        {
            e.Lang, e.Title, e.Id
        });
        await connection.InsertAllAsync(books, trace: new MyCustomTrace(), batchSize: 3, fields: fields);

        // Read
        var q = await connection.QueryAsync<Book>(x => x.Lang == "en", trace: new MyCustomTrace());
        var entities = q.ToList();
        Console.WriteLine(entities.Count);
        foreach (var ent in entities)
        {
            Console.WriteLine($"lang={ent.Lang} title={ent.Title}");
        }
    }
}