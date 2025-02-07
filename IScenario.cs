using System.Diagnostics;
using System.Text;

namespace DemoDatabase;

public interface IScenario
{
    Task ExecuteAsync(string connectionString, string policy);
}

public interface IBenchmarkStrategy
{
    string Name { get; }
    Task InsertBulkAsync(List<Book> chunk);
}

public static class ScenarioHelper
{
    public static int Iteration { get; set; } = 10;
    public static int ChunkSize { get; set; } = 100;

    public static async Task RunBenchmarkAsync(IBenchmarkStrategy strategy)
    {
        var books = Book.CreateMany(Iteration * ChunkSize);
        var chunks = books.Chunk(ChunkSize).Select(x => x.ToList()).ToList();

        var allocatedBytesBefore = GC.GetAllocatedBytesForCurrentThread();

        var stopwatch = Stopwatch.StartNew();
        foreach (var chunk in chunks)
        {
            await strategy.InsertBulkAsync(chunk);
        }

        var allocatedBytesAfter = GC.GetAllocatedBytesForCurrentThread();
        var allocatedBytes = allocatedBytesAfter - allocatedBytesBefore;
        var allocatedMb = allocatedBytes / 1024 / chunks.Count();

        stopwatch.Stop();
        var totalMillis = stopwatch.ElapsedMilliseconds;
        var chunkMillis = totalMillis / chunks.Count();

        var name = strategy.Name;
        var sb = new StringBuilder();
        if (name.Length > 16)
            sb.Append($"{name}\t");
        else if (name.Length > 8)
            sb.Append($"{name}\t\t");
        else
            sb.Append($"{name}\t\t\t");

        sb.Append($"iteration={Iteration}\t");
        sb.Append($"chunkSize={ChunkSize}\t");
        sb.Append($"total={totalMillis} ms\t");
        sb.Append($"chunk={chunkMillis} ms\t");
        sb.Append($"allocated={allocatedMb} KB");
        Console.WriteLine(sb.ToString());
    }
}