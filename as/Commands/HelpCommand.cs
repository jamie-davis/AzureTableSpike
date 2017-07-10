using ConsoleToolkit.CommandLineInterpretation.ConfigurationAttributes;

namespace asSpike.Commands
{
    [Command]
    [Description("Display command help")]
    public class HelpCommand
    {
        [Positional(DefaultValue = null)]
        [Description("Command on which help is required")]
        public string Topic { get; set; }
    }
}
