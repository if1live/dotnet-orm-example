using DemoDatabase;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Scenario_EFCore;

public class BloggingContext : DbContext
{
    public DbSet<Book> Books { get; set; }

    private readonly string _connectionString;

    public BloggingContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    // The following configures EF to create a Sqlite database file in the
    // special "local" folder for your platform.
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        var engine = DatabaseHelper.SelectEngine(_connectionString);
        if (engine == DatabaseEngine.Sqlite)
        {
            options.UseSqlite(_connectionString);
        }
        else
        {
            var serverVersion = new MySqlServerVersion(new Version(8, 0, 40));
            options.UseMySql(_connectionString, serverVersion);
        }

        options
            // .LogTo(Console.WriteLine, LogLevel.Information)
            // .EnableSensitiveDataLogging()
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    }
}

internal class PureStrategy(BloggingContext dbContext) : IBenchmarkStrategy
{
    public string Name => "EFCore.Pure";
    
    public async Task InsertBulkAsync(List<Book> chunk)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        await dbContext.AddRangeAsync(chunk);
        await dbContext.SaveChangesAsync();
        await transaction.CommitAsync();
    }
}

// EFCore.BulkExtensions
// mysql 사용시 local-infile 옵션이 켜져야한다.
internal class EFCoreBulkExtensionsStrategy(BloggingContext dbContext) : IBenchmarkStrategy
{
    public string Name => "EFCore.BulkExtensions";
    
    public async Task InsertBulkAsync(List<Book> chunk)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        await dbContext.BulkInsertAsync(chunk);
        await transaction.CommitAsync();
    }
}

public class Executor : IScenario
{
    public async Task ExecuteAsync(string connectionString, string policy)
    {
        await using var db = new BloggingContext(connectionString);
        await db.Database.EnsureCreatedAsync();

        await db.Books.ExecuteDeleteAsync();

        IBenchmarkStrategy strategy = policy switch
        {
            "Pure" => new PureStrategy(db),
            "EFCore.BulkExtensions" => new EFCoreBulkExtensionsStrategy(db),
            _ => throw new ArgumentException("not supported policy")
        };
        await ScenarioHelper.RunBenchmarkAsync(strategy);

        // Read
        var entities = await db.Books.Where(x => x.Lang == "en").ToListAsync();
        Console.WriteLine(entities.Count);
    }
}