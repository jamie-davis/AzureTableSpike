using System;
using System.Collections.Generic;
using System.Linq;
using ApprovalTests;
using ApprovalTests.Reporters;
using Microsoft.WindowsAzure.Storage.Table;
using NUnit.Framework;
using StorageTests.TestUtilities;
using TestConsole.OutputFormatting;
using TestConsoleLib;
using TestStorage.FilterStrings;

namespace StorageTests.TestStorage.FilterStrings
{
    [TestFixture]
    [UseReporter(typeof(CustomReporter))]
    public class TestFilterClause
    {
        [Test]
        public void FilterStringsAreParsed()
        {
            //Arrange
            var buffer = new OutputBuffer();
            buffer.BufferWidth = 120;
            var output = new Output(buffer);
            
            var strings = new[]
            {
                "var eq 1",
                "var eq 1 and var2 eq 'thing'",
                "(var eq 1)",
                "(var eq 1 or var2 eq 'thing') and var3 gt 56",
                "varg eq guid'9E37E338-27B2-4F32-AFFA-DC74F017AF1D' or (varL ge 45L and var3 ne 35.4)",
                "eq eq eq",
                "(PartitionKey eq '2017-07-30T21:15:09') and((Alpha eq 'Delta') and((Delta eq 'Alpha') or(Delta eq 'Beta')))",
                "(PartitionKey eq '2017-07-30T21:15:09') and((Alpha eq 'Delta') and((Delta eq 'Alpha') or(Delta eq 'Gamma')))",
                "PartitionKey eq '2017-07-30T21:15:09' and Alpha eq 'Delta' and (Delta eq 'Alpha' or Delta eq 'Beta')",
                "PartitionKey eq '2017-07-30T21:15:09' and Alpha eq 'Delta' and (Delta eq 'Alpha' or Delta eq 'Gamma')",
                "PartitionKey eq '2017-07-30T21:15:09' and (Alpha eq 'Delta' and Delta eq 'Alpha' or Delta eq 'Beta')",
                "PartitionKey eq '2017-07-30T21:15:09' and (Alpha eq 'Delta' and Delta eq 'Alpha' or Delta eq 'Gamma')",
            };

            var data = new Dictionary<string, EntityProperty>
            {
                {"var", new EntityProperty(1)},
                {"var2", new EntityProperty("thing")},
                {"var3", new EntityProperty(99.5)},
                {"eq", new EntityProperty("equals")},
                {"varg", new EntityProperty(Guid.Parse("9E37E338-27B2-4F32-AFFA-DC74F017AF1D"))},
                {"varL", new EntityProperty(long.MaxValue)},
                {"PartitionKey", new EntityProperty("2017-07-30T21:15:09")},
                {"Alpha", new EntityProperty("Delta")},
                {"Delta", new EntityProperty("Beta")},
            };

            output.FormatObject(data.Select(d => new { VariableName = d.Key, Value = d.Value.PropertyAsObject.ToString() }));

            //Act
            foreach (var filter in strings)
            {
                var parseResult = FilterStringParser.Parse(filter);

                output.WrapLine($@"Filter string: {filter}");
                output.WrapLine(parseResult.ToString());
                output.WriteLine();
                var result = parseResult.Root.Execute(data, out string error);
                output.WrapLine($"Result = {error ?? result.ToString()}");
                output.WriteLine();
                output.WriteLine();
            }

            //Assert
            Approvals.Verify(output.Report);
        }
    }
}