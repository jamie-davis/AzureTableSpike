using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Threading;
using ConsoleToolkit.CommandLineInterpretation.ConfigurationAttributes;
using ConsoleToolkit.ConsoleIO;
using Microsoft.WindowsAzure.Storage.Table;

namespace asSpike.Commands.TestCommand
{
    [Command]
    [Description("Run a test cycle against the test table")]
    public class TestCommand : BaseCommand
    {
        [CommandHandler]
        public async void Handle(IConsoleAdapter console, IErrorAdapter error)
        {
            try
            {

                var table = TableContextFactory.Get(this, Constants.TestTable, true);
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
                            RowKey = variables[1] + " " + Guid.NewGuid().ToString()
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
                entities[7].Alpha = "Updated (Batch)";
                table.BatchUpdate(entities[7]);
                entities[8].Beta = "Updated (Batch)";
                table.BatchUpdate(entities[8]);

                table.BatchExecuteAsync().Wait();

                console.WrapLine("Performing individual delete");

                table.DeleteAsync(entities[9]).Wait();

                console.WrapLine("Retrieving deleted item");

                var deletedItem = table.GetAsync<TestEntity>(entities[9].PartitionKey, entities[9].RowKey).Result;
                if (deletedItem == null)
                    console.WrapLine("Deleted item not found");
                else
                    console.WrapLine("Deleted item found".Red());

                console.WrapLine("Performing delete again");
                try
                {
                    table.DeleteAsync(entities[9]).Wait();
                }
                catch
                {
                    console.WrapLine("Caught exception");
                }

                console.WrapLine("Performing individual update");
                entities[10].Beta = "Updated (Individual)";
                table.UpdateAsync(entities[10]).Wait();

                console.WrapLine("Retrieving test partition:");

                var query = new TableQuery<TestEntity>();
                query.FilterString = TableQuery.GenerateFilterCondition("PartitionKey", "eq", partition);
                var items = table.Query(query,
                        e => error.WrapLine($"Unable to complete query due to exception:\r\n{e.Message}"))
                    .Select(i => new {i.Alpha, i.Beta, i.Charlie, i.Delta, i.Echo})
                    .OrderBy(i => i.Alpha)
                    .ThenBy(i => i.Beta)
                    .ThenBy(i => i.Charlie)
                    .ThenBy(i => i.Delta)
                    .ThenBy(i => i.Echo);
                console.FormatTable(items);
                console.WriteLine();
                console.WrapLine("Running test query:");

                var whereForQuery = $"PartitionKey eq '{partition}' and (Alpha eq 'Delta' or Alpha eq 'Alpha' and Delta eq 'Beta')";

                var queryWithWhere = new TableQuery<TestEntity>().Where(whereForQuery);

                var resultWithWhere = table.Query(queryWithWhere,
                        e => error.WrapLine($"Unable to complete query due to exception:\r\n{e.Message}"))
                    .Select(i => new {i.Alpha, i.Beta, i.Charlie, i.Delta, i.Echo})
                    .OrderBy(i => i.Alpha)
                    .ThenBy(i => i.Beta)
                    .ThenBy(i => i.Charlie)
                    .ThenBy(i => i.Delta)
                    .ThenBy(i => i.Echo);

                console.WrapLine(whereForQuery);
                console.FormatTable(resultWithWhere);

                console.WriteLine();
                console.WrapLine("Dynamic query (same where)");

                var dynamicQ = new TableQuery<DynamicTableEntity> {SelectColumns = new List<string> {"Alpha", "Charlie"}};
                var dynamicItems = table.CreateDynamicQuery(dynamicQ.Where(whereForQuery),
                        e => error.WrapLine($"Unable to complete query due to exception:\r\n{e.Message}"))
                    .Select(a =>
                        new
                        {
                            Alpha = a.Properties["Alpha"].StringValue,
                            Charlie = a.Properties["Charlie"].StringValue,
                        })
                    .OrderBy(i => i.Alpha)
                    .ThenBy(i => i.Charlie)
                    .ToList();
                console.FormatTable(dynamicItems);

                console.WrapLine("Done");
            }
            catch (Exception e)
            {
                error.WrapLine(e.ToString().Red());
                throw;
            }
        }
    }
}
