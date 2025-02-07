using DemoDatabase;

var executor_dapper = new Scenario_Dapper.Executor();
var executor_dommel = new Scenario_Dommel.Executor();
var executor_efcore = new Scenario_EFCore.Executor();
var executor_querybuilder = new Scenario_QueryBuilder.Executor();
var executor_repodb = new Scenario_RepoDB.Executor();

var table = new Dictionary<string, IScenario>()
{
    ["dapper"] = executor_dapper,
    ["dommel"] = executor_dommel,
    ["efcore"] = executor_efcore,
    ["querybuilder"] = executor_querybuilder,
    ["repodb"] = executor_repodb,
};

// var connectionString = "Data Source=hello.db";
var connectionString = "Server=localhost;Database=localhost_dev;User=localhost_dev;Password=localhost_dev;";

if (args.Length == 0)
{
    await executor_efcore.ExecuteAsync(connectionString);
    await executor_repodb.ExecuteAsync(connectionString);
    await executor_dommel.ExecuteAsync(connectionString);
    await executor_querybuilder.ExecuteAsync(connectionString);
    return;
}

var name = args[^1];
if (table.TryGetValue(name, out var scenario))
{
    await scenario.ExecuteAsync(connectionString);
}
else
    Console.WriteLine($"Invalid name: {name}");