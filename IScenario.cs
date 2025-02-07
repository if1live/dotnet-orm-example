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
    public static async Task RunBenchmarkAsync(IBenchmarkStrategy strategy)
    {
        var iteration = 100;
        var chunkSize = 100;

        var books = Book.CreateMany(iteration * chunkSize);
        var chunks = books.Chunk(chunkSize).Select(x => x.ToList()).ToList();

        var allocatedBytesBefore = GC.GetAllocatedBytesForCurrentThread();

        var stopwatch = Stopwatch.StartNew();
        foreach (var chunk in chunks)
        {
            await strategy.InsertBulkAsync(chunk);
        }

        var allocatedBytesAfter = GC.GetAllocatedBytesForCurrentThread();
        var allocatedBytes = allocatedBytesAfter - allocatedBytesBefore;
        var allocatedMb = allocatedBytes / 1024 / 1024;

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

        sb.Append($"iteration={iteration}\t");
        sb.Append($"chunkSize={chunkSize}\t");
        sb.Append($"total={totalMillis} ms\t");
        sb.Append($"chunk={chunkMillis} ms\t");
        sb.Append($"allocated={allocatedMb} MB");
        Console.WriteLine(sb.ToString());
    }
}