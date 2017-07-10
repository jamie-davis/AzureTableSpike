using ConsoleToolkit.ApplicationStyles;
using ConsoleToolkit;
using asSpike.Commands;

namespace asSpike
{
    class Program : CommandDrivenApplication
    {
        static void Main(string[] args)
        {
            Toolkit.Execute<Program>(args);
        }

        protected override void Initialise()
        {
            base.HelpCommand<HelpCommand>(c => c.Topic);
            base.Initialise();
        }
    }
}
