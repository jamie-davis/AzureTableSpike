using System;
using System.Collections.Generic;
using ApprovalTests;
using ApprovalTests.Reporters;
using Microsoft.WindowsAzure.Storage.Table;
using NUnit.Framework;
using StorageTests.TestUtilities;
using TestConsoleLib;
using TestStorage.FilterStrings;
using TestStorage.FilterStrings.LexicalAnalysis;

namespace StorageTests.TestStorage.FilterStrings
{
    [TestFixture]
    [UseReporter(typeof(CustomReporter))]
    public class TestTokenValue
    {
        [Test]
        public void TokenEqualComparisonsWorkCorrectly()
        {
            //Arrange
            var output = new Output();

            var dateTimeString = "2017-07-30 13:00:44";
            var dateTimeString2 = "2017-07-30 13:00:45";
            var dateTime = DateTime.Parse(dateTimeString);
            var guid = Guid.Parse("9E37E338-27B2-4F32-AFFA-DC74F017AF1D");
            var guid2 = Guid.Parse("9E37E338-27B2-4F32-AFFA-DC74F017AF1E");
            var stringValue = "test string";
            var stringValue2 = "test strinG";
            var data = new Dictionary<string, EntityProperty>
            {
                {"int1", new EntityProperty(1)},
                {"dbl1", new EntityProperty(1.0)},
                {"long1", new EntityProperty(1L)},
                {"dt1", new EntityProperty(dateTime)},
                {"guid1", new EntityProperty(guid)},
                {"string1", new EntityProperty(stringValue)},
            };


            var tests = new[]
            {
                new { Token = TokenValue.FromInt("1"), Variable = "int1"},
                new { Token = TokenValue.FromDouble("1.0"), Variable = "dbl1"},
                new { Token = TokenValue.FromLong("1"), Variable = "long1"},
                new { Token = TokenValue.FromDateTime(dateTimeString), Variable = "dt1"},
                new { Token = TokenValue.FromGuid(guid.ToString()), Variable = "guid1"},
                new { Token = TokenValue.FromString(stringValue), Variable = "string1"},
                new { Token = TokenValue.FromInt("2"), Variable = "int1"},
                new { Token = TokenValue.FromDouble("1.01"), Variable = "dbl1"},
                new { Token = TokenValue.FromLong("2"), Variable = "long1"},
                new { Token = TokenValue.FromDateTime(dateTimeString2), Variable = "dt1"},
                new { Token = TokenValue.FromGuid(guid2.ToString()), Variable = "guid1"},
                new { Token = TokenValue.FromString(stringValue2), Variable = "string1"},
            };

            //Act
            foreach (var test in tests)
            {
                var variableValue = new TokenValue(data[test.Variable]);
                var compareOp = new Token(TokenType.CompEq, "eq");
                var result = test.Token.Compare(compareOp, variableValue, out string _);
                output.WrapLine($@"{test.Token.ValueString()} {compareOp.Text} {variableValue.ValueString()} => {result}");
            }

            //Assert
            Approvals.Verify(output.Report);
        }

        [Test]
        public void TokenNotEqualComparisonsWorkCorrectly()
        {
            //Arrange
            var output = new Output();

            var dateTimeString = "2017-07-30 13:00:44";
            var dateTimeString2 = "2017-07-30 13:00:45";
            var dateTime = DateTime.Parse(dateTimeString);
            var guid = Guid.Parse("9E37E338-27B2-4F32-AFFA-DC74F017AF1D");
            var guid2 = Guid.Parse("9E37E338-27B2-4F32-AFFA-DC74F017AF1E");
            var stringValue = "test string";
            var stringValue2 = "test strinG";
            var data = new Dictionary<string, EntityProperty>
            {
                {"int1", new EntityProperty(1)},
                {"dbl1", new EntityProperty(1.0)},
                {"long1", new EntityProperty(1L)},
                {"dt1", new EntityProperty(dateTime)},
                {"guid1", new EntityProperty(guid)},
                {"string1", new EntityProperty(stringValue)},
            };


            var tests = new[]
            {
                new { Token = TokenValue.FromInt("1"), Variable = "int1"},
                new { Token = TokenValue.FromDouble("1.0"), Variable = "dbl1"},
                new { Token = TokenValue.FromLong("1"), Variable = "long1"},
                new { Token = TokenValue.FromDateTime(dateTimeString), Variable = "dt1"},
                new { Token = TokenValue.FromGuid(guid.ToString()), Variable = "guid1"},
                new { Token = TokenValue.FromString(stringValue), Variable = "string1"},
                new { Token = TokenValue.FromInt("2"), Variable = "int1"},
                new { Token = TokenValue.FromDouble("1.01"), Variable = "dbl1"},
                new { Token = TokenValue.FromLong("2"), Variable = "long1"},
                new { Token = TokenValue.FromDateTime(dateTimeString2), Variable = "dt1"},
                new { Token = TokenValue.FromGuid(guid2.ToString()), Variable = "guid1"},
                new { Token = TokenValue.FromString(stringValue2), Variable = "string1"},
            };

            //Act
            foreach (var test in tests)
            {
                var variableValue = new TokenValue(data[test.Variable]);
                var compareOp = new Token(TokenType.CompNe, "ne");
                var result = test.Token.Compare(compareOp, variableValue, out string _);
                output.WrapLine($@"{test.Token.ValueString()} {compareOp.Text} {variableValue.ValueString()} => {result}");
            }

            //Assert
            Approvals.Verify(output.Report);
        }

        [Test]
        public void TokenGreaterComparisonsWorkCorrectly()
        {
            //Arrange
            var output = new Output();

            var dateTimeString = "2017-07-30 13:00:44";
            var dateTimeString2 = "2017-07-30 13:00:45";
            var dateTime = DateTime.Parse(dateTimeString);
            var guid = Guid.Parse("9E37E338-27B2-4F32-AFFA-DC74F017AF1D");
            var guid2 = Guid.Parse("9E37E338-27B2-4F32-AFFA-DC74F017AF1E");
            var stringValue = "test string";
            var stringValue2 = "west string";
            var data = new Dictionary<string, EntityProperty>
            {
                {"int1", new EntityProperty(1)},
                {"dbl1", new EntityProperty(1.0)},
                {"long1", new EntityProperty(1L)},
                {"dt1", new EntityProperty(dateTime)},
                {"guid1", new EntityProperty(guid)},
                {"string1", new EntityProperty(stringValue)},
            };


            var tests = new[]
            {
                new { Token = TokenValue.FromInt("1"), Variable = "int1"},
                new { Token = TokenValue.FromDouble("1.0"), Variable = "dbl1"},
                new { Token = TokenValue.FromLong("1"), Variable = "long1"},
                new { Token = TokenValue.FromDateTime(dateTimeString), Variable = "dt1"},
                new { Token = TokenValue.FromGuid(guid.ToString()), Variable = "guid1"},
                new { Token = TokenValue.FromString(stringValue), Variable = "string1"},
                new { Token = TokenValue.FromInt("2"), Variable = "int1"},
                new { Token = TokenValue.FromDouble("1.01"), Variable = "dbl1"},
                new { Token = TokenValue.FromLong("2"), Variable = "long1"},
                new { Token = TokenValue.FromDateTime(dateTimeString2), Variable = "dt1"},
                new { Token = TokenValue.FromGuid(guid2.ToString()), Variable = "guid1"},
                new { Token = TokenValue.FromString(stringValue2), Variable = "string1"},
            };

            //Act
            foreach (var test in tests)
            {
                var variableValue = new TokenValue(data[test.Variable]);
                var compareOp = new Token(TokenType.CompGt, "gt");
                var result = test.Token.Compare(compareOp, variableValue, out string _);
                output.WrapLine($@"{test.Token.ValueString()} {compareOp.Text} {variableValue.ValueString()} => {result}");
            }

            //Assert
            Approvals.Verify(output.Report);
        }

        [Test]
        public void TokenLesserComparisonsWorkCorrectly()
        {
            //Arrange
            var output = new Output();

            var dateTimeString = "2017-07-30 13:00:44";
            var dateTimeString2 = "2017-07-30 13:00:43";
            var dateTime = DateTime.Parse(dateTimeString);
            var guid = Guid.Parse("9E37E338-27B2-4F32-AFFA-DC74F017AF1D");
            var guid2 = Guid.Parse("9E37E338-27B2-4F32-AFFA-DC74F017AF1C");
            var stringValue = "test string";
            var stringValue2 = "sest string";
            var data = new Dictionary<string, EntityProperty>
            {
                {"int1", new EntityProperty(1)},
                {"dbl1", new EntityProperty(1.0)},
                {"long1", new EntityProperty(1L)},
                {"dt1", new EntityProperty(dateTime)},
                {"guid1", new EntityProperty(guid)},
                {"string1", new EntityProperty(stringValue)},
            };


            var tests = new[]
            {
                new { Token = TokenValue.FromInt("1"), Variable = "int1"},
                new { Token = TokenValue.FromDouble("1.0"), Variable = "dbl1"},
                new { Token = TokenValue.FromLong("1"), Variable = "long1"},
                new { Token = TokenValue.FromDateTime(dateTimeString), Variable = "dt1"},
                new { Token = TokenValue.FromGuid(guid.ToString()), Variable = "guid1"},
                new { Token = TokenValue.FromString(stringValue), Variable = "string1"},
                new { Token = TokenValue.FromInt("0"), Variable = "int1"},
                new { Token = TokenValue.FromDouble("0.99"), Variable = "dbl1"},
                new { Token = TokenValue.FromLong("0"), Variable = "long1"},
                new { Token = TokenValue.FromDateTime(dateTimeString2), Variable = "dt1"},
                new { Token = TokenValue.FromGuid(guid2.ToString()), Variable = "guid1"},
                new { Token = TokenValue.FromString(stringValue2), Variable = "string1"},
            };

            //Act
            foreach (var test in tests)
            {
                var variableValue = new TokenValue(data[test.Variable]);
                var compareOp = new Token(TokenType.CompLt, "lt");
                var result = test.Token.Compare(compareOp, variableValue, out string _);
                output.WrapLine($@"{test.Token.ValueString()} {compareOp.Text} {variableValue.ValueString()} => {result}");
            }

            //Assert
            Approvals.Verify(output.Report);
        }

        [Test]
        public void TokenGreaterEqualComparisonsWorkCorrectly()
        {
            //Arrange
            var output = new Output();

            var dateTimeString = "2017-07-30 13:00:44";
            var dateTimeString2 = "2017-07-30 13:00:45";
            var dateTimeString3 = "2017-07-30 13:00:43";
            var dateTime = DateTime.Parse(dateTimeString);
            var guid = Guid.Parse("9E37E338-27B2-4F32-AFFA-DC74F017AF1D");
            var guid2 = Guid.Parse("9E37E338-27B2-4F32-AFFA-DC74F017AF1E");
            var guid3 = Guid.Parse("9E37E338-27B2-4F32-AFFA-DC74F017AF1C");
            var stringValue = "test string";
            var stringValue2 = "west string";
            var stringValue3 = "sest string";
            var data = new Dictionary<string, EntityProperty>
            {
                {"int1", new EntityProperty(1)},
                {"dbl1", new EntityProperty(1.0)},
                {"long1", new EntityProperty(1L)},
                {"dt1", new EntityProperty(dateTime)},
                {"guid1", new EntityProperty(guid)},
                {"string1", new EntityProperty(stringValue)},
            };


            var tests = new[]
            {
                new { Token = TokenValue.FromInt("1"), Variable = "int1"},
                new { Token = TokenValue.FromDouble("1.0"), Variable = "dbl1"},
                new { Token = TokenValue.FromLong("1"), Variable = "long1"},
                new { Token = TokenValue.FromDateTime(dateTimeString), Variable = "dt1"},
                new { Token = TokenValue.FromGuid(guid.ToString()), Variable = "guid1"},
                new { Token = TokenValue.FromString(stringValue), Variable = "string1"},
                new { Token = TokenValue.FromInt("2"), Variable = "int1"},
                new { Token = TokenValue.FromDouble("1.01"), Variable = "dbl1"},
                new { Token = TokenValue.FromLong("2"), Variable = "long1"},
                new { Token = TokenValue.FromDateTime(dateTimeString2), Variable = "dt1"},
                new { Token = TokenValue.FromGuid(guid2.ToString()), Variable = "guid1"},
                new { Token = TokenValue.FromString(stringValue2), Variable = "string1"},
                new { Token = TokenValue.FromInt("0"), Variable = "int1"},
                new { Token = TokenValue.FromDouble("0.99"), Variable = "dbl1"},
                new { Token = TokenValue.FromLong("0"), Variable = "long1"},
                new { Token = TokenValue.FromDateTime(dateTimeString3), Variable = "dt1"},
                new { Token = TokenValue.FromGuid(guid3.ToString()), Variable = "guid1"},
                new { Token = TokenValue.FromString(stringValue3), Variable = "string1"},
            };

            //Act
            foreach (var test in tests)
            {
                var variableValue = new TokenValue(data[test.Variable]);
                var compareOp = new Token(TokenType.CompGe, "ge");
                var result = test.Token.Compare(compareOp, variableValue, out string _);
                output.WrapLine($@"{test.Token.ValueString()} {compareOp.Text} {variableValue.ValueString()} => {result}");
            }

            //Assert
            Approvals.Verify(output.Report);
        }

        [Test]
        public void TokenLesserEqualComparisonsWorkCorrectly()
        {
            //Arrange
            var output = new Output();

            var dateTimeString = "2017-07-30 13:00:44";
            var dateTimeString2 = "2017-07-30 13:00:45";
            var dateTimeString3 = "2017-07-30 13:00:43";
            var dateTime = DateTime.Parse(dateTimeString);
            var guid = Guid.Parse("9E37E338-27B2-4F32-AFFA-DC74F017AF1D");
            var guid2 = Guid.Parse("9E37E338-27B2-4F32-AFFA-DC74F017AF1E");
            var guid3 = Guid.Parse("9E37E338-27B2-4F32-AFFA-DC74F017AF1C");
            var stringValue = "test string";
            var stringValue2 = "west string";
            var stringValue3 = "sest string";
            var data = new Dictionary<string, EntityProperty>
            {
                {"int1", new EntityProperty(1)},
                {"dbl1", new EntityProperty(1.0)},
                {"long1", new EntityProperty(1L)},
                {"dt1", new EntityProperty(dateTime)},
                {"guid1", new EntityProperty(guid)},
                {"string1", new EntityProperty(stringValue)},
            };


            var tests = new[]
            {
                new { Token = TokenValue.FromInt("1"), Variable = "int1"},
                new { Token = TokenValue.FromDouble("1.0"), Variable = "dbl1"},
                new { Token = TokenValue.FromLong("1"), Variable = "long1"},
                new { Token = TokenValue.FromDateTime(dateTimeString), Variable = "dt1"},
                new { Token = TokenValue.FromGuid(guid.ToString()), Variable = "guid1"},
                new { Token = TokenValue.FromString(stringValue), Variable = "string1"},
                new { Token = TokenValue.FromInt("2"), Variable = "int1"},
                new { Token = TokenValue.FromDouble("1.01"), Variable = "dbl1"},
                new { Token = TokenValue.FromLong("2"), Variable = "long1"},
                new { Token = TokenValue.FromDateTime(dateTimeString2), Variable = "dt1"},
                new { Token = TokenValue.FromGuid(guid2.ToString()), Variable = "guid1"},
                new { Token = TokenValue.FromString(stringValue2), Variable = "string1"},
                new { Token = TokenValue.FromInt("0"), Variable = "int1"},
                new { Token = TokenValue.FromDouble("0.99"), Variable = "dbl1"},
                new { Token = TokenValue.FromLong("0"), Variable = "long1"},
                new { Token = TokenValue.FromDateTime(dateTimeString3), Variable = "dt1"},
                new { Token = TokenValue.FromGuid(guid3.ToString()), Variable = "guid1"},
                new { Token = TokenValue.FromString(stringValue3), Variable = "string1"},
            };

            //Act
            foreach (var test in tests)
            {
                var variableValue = new TokenValue(data[test.Variable]);
                var compareOp = new Token(TokenType.CompLe, "le");
                var result = test.Token.Compare(compareOp, variableValue, out string _);
                output.WrapLine($@"{test.Token.ValueString()} {compareOp.Text} {variableValue.ValueString()} => {result}");
            }

            //Assert
            Approvals.Verify(output.Report);
        }
    }
}
