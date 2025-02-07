using System.Data;
using Bogus;
using DemoDatabase;
using Microsoft.Data.Sqlite;
using MySql.Data.MySqlClient;
using RepoDb;
using RepoDb.Interfaces;

namespace Scenario_RepoDB;

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
        // Console.WriteLine($"BeforeExecutionAsync: {log.Statement}");
        return Task.CompletedTask;
    }

    public Task AfterExecutionAsync<TResult>(ResultTraceLog<TResult> log,
        CancellationToken cancellationToken = new CancellationToken())
    {
        // Console.WriteLine($"AfterExecutionAsync: {log.Result}, TotalTime: {log.ExecutionTime.TotalSeconds} second(s)");
        return Task.CompletedTask;
    }
}

class Strategy(IDbConnection connection) : IBenchmarkStrategy
{
    public async Task InsertBulkAsync(List<Book> chunk)
    {
        using var transaction = (await connection.EnsureOpenAsync()).BeginTransaction();
        await connection.InsertAllAsync(chunk, transaction: transaction);
        transaction.Commit();
    }
}

public class Executor : IScenario
{
    public async Task ExecuteAsync(string connectionString)
    {
        var engine = DatabaseHelper.SelectEngine(connectionString);
        if (engine == DatabaseEngine.Sqlite)
        {
            GlobalConfiguration.Setup().UseSqlite();
        }
        else
        {
            GlobalConfiguration.Setup().UseMySql();
        }

        using IDbConnection connection = engine == DatabaseEngine.Sqlite
            ? new SqliteConnection(connectionString)
            : new MySqlConnection(connectionString);
        connection.Open();

        await connection.DeleteAllAsync<Book>();

        // 문제: n개의 insert + select의 반복된 쿼리 생성. 레코드 정보 전체를 제공하니까 select 필요 없고, insert 하나로 다 넣고 싶은데
        // BulkAsync를 RepoDB에서 지원하지만 mssql만 된다고 문서에 써있음
        var strategy = new Strategy(connection);
        await ScenarioHelper.RunBenchmarkAsync(strategy);

        // Read
        var q = await connection.QueryAsync<Book>(x => x.Lang == "en");
        var entities = q.ToList();
        Console.WriteLine(entities.Count);
    }
}