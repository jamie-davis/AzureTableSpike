using AzureStorage.DataAccess;
using ConsoleToolkit.CommandLineInterpretation.ConfigurationAttributes;
using ConsoleToolkit.ConsoleIO;
using Microsoft.WindowsAzure.Storage;

namespace asSpike.Commands.TablesCommand
{
    [Command]
    [Description("Exercise azure tables")]
    public class TablesCommand
    {
        [CommandHandler]
        public void Handle(IConsoleAdapter console, IErrorAdapter error)
        {
            console.WrapLine(Properties.Settings.Default.ConnectionString.Red());
            var connectionString = Properties.Settings.Default.ConnectionString;
            var context = new TableContext(Constants.ActivityTable, connectionString, tryCreate: true);
        }
    }
}
