using DemoDatabase;

var table = new Dictionary<string, IScenario>()
{
    ["dapper"] = new Scenario_Dapper.Executor(),
    ["dommel"] = new Scenario_Dommel.Executor(),
    ["efcore"] = new Scenario_EFCore.Executor(),
    ["querybuilder"] = new Scenario_QueryBuilder.Executor(),
    ["repodb"] = new Scenario_RepoDB.Executor(),
};

var name = args[^1];
if (table.TryGetValue(name, out var scenario))
    await scenario.ExecuteAsync();
else
    Console.WriteLine($"Invalid name: {name}");