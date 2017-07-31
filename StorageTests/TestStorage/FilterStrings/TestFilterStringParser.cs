using ApprovalTests;
using ApprovalTests.Reporters;
using NUnit.Framework;
using StorageTests.TestUtilities;
using TestConsoleLib;
using TestStorage.FilterStrings;

namespace StorageTests.TestStorage.FilterStrings
{
    [TestFixture]
    [UseReporter(typeof(CustomReporter))]
    public class TestFilterStringParser
    {
        [Test]
        public void FilterStringsAreParsed()
        {
            //Arrange
            var output = new Output();
            var strings = new[]
                {
                    "var eq 1",
                    "var eq 1 and var2 eq 'thing'",
                    "(var eq 1)",
                    "(var eq 1 or var2 eq 'thing') and var3 gt 56",
                    "var eq guid'9E37E338-27B2-4F32-AFFA-DC74F017AF1D' or (var2 ge 45L and var3 ne 35.4)",
                    "eq eq eq",
                    "and",
                    "and and and",
                    "(a eq 1",
                    "a eq 1)",
                    "PartitionKey eq '2017-07-30T21:15:09' and (Alpha eq 'Delta' and (Delta eq 'Alpha' or Delta eq 'Beta'))"
                };

            //Act
            foreach (var filter in strings)
            {
                var result = FilterStringParser.Parse(filter);

                output.WrapLine($@"Filter string: {filter}");
                output.WrapLine(result.ToString());
                output.WriteLine();
                output.WriteLine();
            }

            //Assert
            Approvals.Verify(output.Report);
        }
    }
}
