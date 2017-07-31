using ConsoleToolkit.ApplicationStyles;
using ConsoleToolkit;
using asSpike.Commands;
using ConsoleToolkit.ConsoleIO;

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
            Toolkit.SetCommandExceptionHandler((con, err, ex, ob) => err.WrapLine(ex.ToString().Red()));
            base.HelpCommand<HelpCommand>(c => c.Topic);
            base.Initialise();
        }
    }
}
