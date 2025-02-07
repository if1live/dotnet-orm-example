using System.Diagnostics;

namespace DemoDatabase;

public interface IScenario
{
    Task ExecuteAsync(string connectionString);
}

public interface IBenchmarkStrategy
{
    Task InsertBulkAsync(List<Book> chunk);
}

public class ScenarioHelper
{
    public static async Task RunBenchmarkAsync(IBenchmarkStrategy strategy)
    {
        var books = Book.CreateMany(100_00);
        var chunks = books.Chunk(100).Select(x => x.ToList()).ToList();
        // var books = Book.CreateMany(2);
        // var chunks = books.Chunk(2).Select(x => x.ToList());

        var stopwatch = Stopwatch.StartNew();
        foreach (var chunk in chunks)
        {
            await strategy.InsertBulkAsync(chunk);
        }

        stopwatch.Stop();

        var ty = strategy.GetType().Namespace;
        var millis = stopwatch.ElapsedMilliseconds;
        Console.WriteLine($"{ty}: total={millis} ms / chunk={millis / chunks.Count()} ms");
    }
}