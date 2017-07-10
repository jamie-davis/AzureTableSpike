using System;
using System.Collections.Generic;
using System.Linq;
using asSpike.Entities;
using AzureStorage.DataAccess;
using ConsoleToolkit.CommandLineInterpretation.ConfigurationAttributes;
using ConsoleToolkit.ConsoleIO;
using Microsoft.WindowsAzure.Storage.Shared.Protocol;
using Microsoft.WindowsAzure.Storage.Table;

namespace asSpike.Commands.TestCommand
{
    [Command]
    [Description("Run a test cycle against the test table")]
    public class TestCommand
    {
        [CommandHandler]
        public async void Handle(IConsoleAdapter console, IErrorAdapter error)
        {
            var connectionString = Properties.Settings.Default.ConnectionString;
            var table = new TableContext(AzureStorage.DataAccess.Constants.TestTable, connectionString, tryCreate: true);

            var variables = new[]
            {
                "Alpha", "Beta", "Charlie", "Delta", "Echo"
            }.ToList();

            var partition = DateTime.Now.ToString("s");
            console.WrapLine($"Partition key = {partition}");

            var entities = Enumerable.Range(0, 20)
                .Select(e =>
                {
                    var item = new TestEntity
                    {
                        Alpha = variables[0],
                        Beta = variables[1],
                        Charlie = variables[2],
                        Delta = variables[3],
                        Echo = variables[4],
                        PartitionKey = partition,
                        RowKey = variables[1]+" "+Guid.NewGuid().ToString()
                    };
                    var top = variables[0];
                    variables.RemoveAt(0);
                    variables.Add(top);
                    return item;
                }).ToList();

            console.WrapLine("Generating entities using batch add...");
            foreach (var entity in entities)
            {
                table.BatchAdd(entity);
            }

            table.BatchExecuteAsync().Wait();

            console.WrapLine("Performing batch updates");

            table.BatchDelete(entities[5]);
            entities[7].Alpha = "Updated";
            table.BatchUpdate(entities[7]);
            entities[8].Beta = "Updated";
            table.BatchUpdate(entities[8]);

            table.BatchExecuteAsync().Wait();

            console.WrapLine("Retrieving test partition:");

            var query = new TableQuery<TestEntity>();
            query.FilterString = TableQuery.GenerateFilterCondition("PartitionKey", "eq", partition);
            table.Query(query, e => error.WrapLine($"Unable to complete query due to exception:\r\n{e.Message}"));
            console.WrapLine("Done");
        }
    }
}
