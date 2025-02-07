using System.CommandLine;
using DemoDatabase;

var executor_dapper = new Scenario_Dapper.Executor();
var executor_dommel = new Scenario_Dommel.Executor();
var executor_efcore = new Scenario_EFCore.Executor();
var executor_querybuilder = new Scenario_QueryBuilder.Executor();
var executor_repodb = new Scenario_RepoDB.Executor();

var connectionString_sqlite = "Data Source=hello.db";
var connectionString_mysql =
    "Server=localhost;Database=localhost_dev;User=localhost_dev;Password=localhost_dev;AllowLoadLocalInfile=true;";

var dbOption = new Option<string>(
    name: "--db",
    description: "db"
);


var chunkSizeOption = new Option<int>(
    name: "--chunk",
    description: "chunk size",
    getDefaultValue: () => 100
);

var iterationOption = new Option<int>(
    name: "--iteration",
    description: "iteration",
    getDefaultValue: () => 10
);

var rootCommand = new RootCommand("Sample app for System.CommandLine");
rootCommand.AddOption(dbOption);
rootCommand.AddOption(chunkSizeOption);
rootCommand.AddOption(iterationOption);

rootCommand.SetHandler(async (engine, iteration, chunkSize) =>
    {
        ScenarioHelper.Iteration = iteration;
        ScenarioHelper.ChunkSize = chunkSize;
        
        var connectionString = engine switch
        {
            "sqlite" => connectionString_sqlite,
            "mysql" => connectionString_mysql,
            _ => throw new ArgumentException("not supported engine")
        };

        await executor_efcore.ExecuteAsync(connectionString, "Pure");
        await executor_efcore.ExecuteAsync(connectionString, "EFCore.BulkExtensions");
        await executor_repodb.ExecuteAsync(connectionString, "");
        await executor_dapper.ExecuteAsync(connectionString, "");
        await executor_dommel.ExecuteAsync(connectionString, "");
        await executor_querybuilder.ExecuteAsync(connectionString, "");
    },
    dbOption, iterationOption, chunkSizeOption);

return await rootCommand.InvokeAsync(args);