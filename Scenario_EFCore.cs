using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DemoDatabase;
using Scenario_EFCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Scenario_EFCore;

public class BloggingContext : DbContext
{
    public DbSet<Book> Books { get; set; }

    public string DbPath { get; }

    public BloggingContext()
    {
        DbPath = System.IO.Path.Join("hello.db");
    }

    // The following configures EF to create a Sqlite database file in the
    // special "local" folder for your platform.
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options
            .UseSqlite($"Data Source={DbPath}")
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
            .LogTo(Console.WriteLine, LogLevel.Information)
            .EnableSensitiveDataLogging(); // 민감한 데이터도 로깅
    }
}

[Table("book")]
public class Book
{
    public int Id { get; set; }
    public string Lang { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
}

public class Executor : IScenario
{
    public async Task ExecuteAsync()
    {
        await using var db = new BloggingContext();
        await db.Database.EnsureCreatedAsync();

        // Note: This sample requires the database to be created before running.
        Console.WriteLine($"Database path: {db.DbPath}.");

        await db.Books.ExecuteDeleteAsync();

        // 문제: AddRangeAsync를 사용해도 쿼리 2개 호출됨
        // id가 db에서 유도되게 하면 'RETURNING "Id"' 때문에 어쩔수 없다고쳐도
        // 전체 데이터 채워서 insert할때도 쿼리가 나뉘는 문제가 있다
        var b1 = new Book() { Id = 1, Lang = "en", Title = "foo" };
        var b2 = new Book() { Id = 2, Lang = "en", Title = "bar" };
        await db.AddRangeAsync([b1, b2]);
        await db.SaveChangesAsync();

        // Read
        var entities = await db.Books.Where(x => x.Lang == "en").ToListAsync();
        Console.WriteLine(entities.Count);
        foreach (var ent in entities)
        {
            Console.WriteLine($"id={ent.Id}, lang={ent.Lang}");
        }
    }
}