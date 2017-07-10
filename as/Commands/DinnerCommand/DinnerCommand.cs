using System;
using asSpike.Entities;
using AzureStorage.DataAccess;
using ConsoleToolkit.CommandLineInterpretation.ConfigurationAttributes;
using ConsoleToolkit.ConsoleIO;

namespace asSpike.Commands.DinnerCommand
{
    [Command]
    [Description("Add a new activity")]
    public class DinnerCommand
    {
        [Positional]
        [Description("Where the meal occurred")]
        public string Where { get; set; }

        [Positional]
        [Description("The date and time the activity started")]
        public DateTime When { get; set; }

        [Option("c")]
        [Description("Create the table.")]
        public bool CreateTable { get; set; }

        [CommandHandler]
        public void Handle(IConsoleAdapter console, IErrorAdapter error)
        {
            var activity = new Dinner
            {
                Where = Where,
                WhenDidItStart = When,
                Key = Guid.NewGuid()
            };

            var connectionString = Properties.Settings.Default.ConnectionString;
            var context = new TableContext(Constants.DinnerTable, connectionString, tryCreate: CreateTable);
            context.AddAsync(activity).Wait();
        }
    }
}
