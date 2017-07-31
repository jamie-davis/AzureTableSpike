using System.Linq;
using ConsoleToolkit.CommandLineInterpretation.ConfigurationAttributes;
using ConsoleToolkit.ConsoleIO;
using Microsoft.WindowsAzure.Storage.Table;

namespace asSpike.Commands.ActivitiesCommand
{
    [Command]
    [Description("Query activities")]
    public class ActivitiesCommand : BaseCommand
    {

        [CommandHandler]
        public void Handle(IConsoleAdapter console, IErrorAdapter error)
        {
            var context = TableContextFactory.Get(this, Constants.ActivityTable, true);
            var query = new TableQuery<DynamicTableEntity>();
            var items = context.CreateDynamicQuery(query,
                                e => error.WrapLine($"Unable to complete query due to exception:\r\n{e.Message}"))
                .Select(a =>
                    new
                    {
                        Who = PropertyOrEmptyString(a, "Who"),
                        What = PropertyOrEmptyString(a, "What"),
                        WhenDidItStart = PropertyOrEmptyString(a, "WhenDidItStart"),
                        HowLong = PropertyOrEmptyString(a, "HowLong")
                    })
                    .ToList();
            console.FormatTable(items);
        }

        private string PropertyOrEmptyString(DynamicTableEntity dynamicTableEntity, string who)
        {
            EntityProperty value;
            if (!dynamicTableEntity.Properties.TryGetValue(who, out value))
                return string.Empty;

            switch (value.PropertyType)
            {
                case EdmType.String:
                    return value.StringValue;

                case EdmType.Binary:
                    return value.StringValue;

                case EdmType.Boolean:
                    return value.BooleanValue.ToString();

                case EdmType.DateTime:
                    return value.DateTime?.ToString("d") ?? string.Empty;

                case EdmType.Double:
                    return value.DoubleValue?.ToString() ?? string.Empty;

                case EdmType.Guid:
                    return value.GuidValue?.ToString() ?? string.Empty;

                case EdmType.Int32:
                    return value.Int32Value?.ToString() ?? string.Empty;

                case EdmType.Int64:
                    return value.Int64Value?.ToString() ?? string.Empty;

                default:
                    return string.Empty;
            }
        }
    }
}