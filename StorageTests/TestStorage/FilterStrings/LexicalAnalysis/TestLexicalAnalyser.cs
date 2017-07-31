using ApprovalTests;
using ApprovalTests.Reporters;
using NUnit.Framework;
using StorageTests.TestUtilities;
using TestConsole.OutputFormatting;
using TestConsoleLib;
using TestStorage.FilterStrings.LexicalAnalysis;

namespace StorageTests.TestStorage.FilterStrings.LexicalAnalysis
{
    [TestFixture]
    [UseReporter(typeof(CustomReporter))]
    public class TestLexicalAnalyser
    {
        [Test]
        public void LexicalAnalysisTests()
        {
            //Arrange
            var testStrings = new[]
            {
                "var eq 'literal'",
                "(var gt 'literal' and var2 lt 45) or (datevar gt datetime'2017-07-21T22:02:31Z')",
                "var eq guid'9E37E338-27B2-4F32-AFFA-DC74F017AF1D' or (var2 ge 45L and var3 ne 35.4)",
                string.Empty,
                null,
                "\t\tvar        eq\r\n'literal'",
            };

            var output = new Output();

            //Act
            foreach (var testString in testStrings)
            {
                var result = LexicalAnalyser.Analyse(testString);

                output.WrapLine($@"Analysis of ""{testString ?? "NULL"}"":");
                output.FormatTable(result.AsReport(rep => rep
                                                    .AddColumn(r => r.TokenType, cc => cc.LeftAlign())
                                                    .AddColumn(r => r.Text, cc => {}))
                );

                output.WriteLine();
                output.WriteLine();
            }

            //Assert
            Approvals.Verify(output.Report);
        }
    }
}
