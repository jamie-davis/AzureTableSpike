using asSpike.Commands.ActivitiesCommand;
using ConsoleToolkit.CommandLineInterpretation.ConfigurationAttributes;
using ConsoleToolkit.ConsoleIO;

namespace asSpike.Commands.UpdateCommand
{
    //[Command] /**/
    [Description("Update row and field")]
    public class UpdateCommand : BaseCommand
    {
        [Positional]
        [Description("The table to update")]
        public string Table { get; set; }

        [Positional]
        [Description("The field to update")]
        public string Field { get; set; }

        [Positional]
        [Description("The new value")]
        public string Value { get; set; }

        [Option("d")]
        [Description("Convert the value to a date time")]
        public bool DateTime { get; set; }

        [Option("t")]
        [Description("Convert the value to a timespan")]
        public bool TimeSpan { get; set; }

        [CommandHandler]
        public void Handle(IConsoleAdapter console, IErrorAdapter error)
        {
            var context = TableContextFactory.Get(this, Table);
            //table.UpdateAsync()
        }
    }
}
