using System.Diagnostics;
using System.Text;

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
        var iteration = 100;
        var chunkSize = 100;

        var books = Book.CreateMany(iteration * chunkSize);
        var chunks = books.Chunk(chunkSize).Select(x => x.ToList()).ToList();

        var stopwatch = Stopwatch.StartNew();
        var allocatedBytesBefore = GC.GetAllocatedBytesForCurrentThread();
        foreach (var chunk in chunks)
        {
            await strategy.InsertBulkAsync(chunk);
        }

        var allocatedBytesAfter = GC.GetAllocatedBytesForCurrentThread();
        var allocatedBytes = allocatedBytesAfter - allocatedBytesBefore;
        var allocatedMb = allocatedBytes / 1024 / 1024;

        stopwatch.Stop();

        var ty = strategy.GetType().Namespace ?? "";
        var name = ty.Split('_').Last() ?? "BLANK";

        var totalMillis = stopwatch.ElapsedMilliseconds;
        var chunkMillis = totalMillis / chunks.Count();

        var sb = new StringBuilder();
        if (name.Length > 8)
            sb.Append($"{name}\t");
        else
            sb.Append($"{name}\t\t");

        sb.Append($"iteration={iteration}\t");
        sb.Append($"chunkSize={chunkSize}\t");
        sb.Append($"total={totalMillis} ms\t");
        sb.Append($"chunk={chunkMillis} ms\t");
        sb.Append($"allocated={allocatedMb} MB");
        Console.WriteLine(sb.ToString());
    }
}