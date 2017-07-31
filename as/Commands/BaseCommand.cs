using ConsoleToolkit.CommandLineInterpretation.ConfigurationAttributes;

namespace asSpike.Commands
{
    public class BaseCommand
    {
        [Description("Use a fake data store rather than Azure table storage.")]
        [Option("fake", "f")]
        public bool FakeDataStore { get; set; }

        [Option("dev")]
        [Description("Use the development storage emulator connection string.")]
        public bool UseDevStorage { get; set; }
    }
}