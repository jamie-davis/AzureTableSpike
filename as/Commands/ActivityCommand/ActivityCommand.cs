using System;
using asSpike.Entities;
using AzureStorage.DataAccess;
using ConsoleToolkit.CommandLineInterpretation.ConfigurationAttributes;
using ConsoleToolkit.ConsoleIO;

namespace asSpike.Commands.ActivityCommand
{
    [Command]
    [Description("Add a new activity")]
    public class ActivityCommand
    {
        [Positional]
        [Description("The name of the person doing the activity")]
        public string Who { get; set; }

        [Positional]
        [Description("What the activity was")]
        public string What { get; set; }

        [Positional]
        [Description("The date and time the activity started")]
        public DateTime When { get; set; }

        [Positional]
        [Description("The duration of the activity (time span format)")]
        public string HowLong { get; set; }

        [Option("c")]
        [Description("Create the table.")]
        public bool CreateTable { get; set; }

        [CommandHandler]
        public void Handle(IConsoleAdapter console, IErrorAdapter error)
        {
            TimeSpan howLong;
            if (!TimeSpan.TryParse(HowLong, out howLong))
            {
                error.WrapLine("Invalid timespan format.");
                Environment.ExitCode = -100;
                return;
            }

            var activity = new Activity()
            {
                Who = Who,
                What = What,
                WhenDidItStart = When,
                HowLong = howLong,
                Key = Guid.NewGuid()
            };

            var connectionString = Properties.Settings.Default.ConnectionString;
            var context = new TableContext(Constants.ActivityTable, connectionString, tryCreate: CreateTable);
            context.UpdateAsync(activity).Wait();
        }
    }
}
