using System;
using System.IO;
using System.Linq;
using System.Reflection;
using ConsoleToolkit.CommandLineInterpretation.ConfigurationAttributes;
using ConsoleToolkit.ConsoleIO;

namespace asSpike.Commands.BlobCommand
{
    [Command]
    [Description("Perform Blob tests")]
    class BlobCommand : BaseCommand
    {
        [CommandHandler]
        public void Handle(IConsoleAdapter console, IErrorAdapter error)
        {
            var context = BlobContextFactory.Get(this, true);

            var container = context.GetContainer("test");

            console.WrapLine("Getting a shared access signature:");
            console.WrapLine(container.GetSharedAccessSignature(30).Cyan());

            console.WrapLine("Uploading to test.txt");

            container.UploadText("test.txt", "my test text");

            console.WriteLine();
            console.WrapLine("Listing blobs");

            console.FormatTable(container.ListBlobs("").Select(s => new {BlobName = s}));

            console.WrapLine("Deleting blob");
            container.DeleteBlob("test.txt");
            console.WriteLine();
            console.WrapLine("Listing blobs again");
            console.FormatTable(container.ListBlobs("").Select(s => new { BlobName = s }));

            console.WriteLine();
            console.WrapLine("Uploading stream data");

            using (var data = File.OpenRead(Assembly.GetExecutingAssembly().Location))
            {
                container.Upload("exedata", data);
            }

            console.WriteLine();
            console.WrapLine("Downloading stream data");

            using (var stream = container.OpenStream("exedata"))
            {
                var bytesRead = 0;
                var totalBytesRead = 0;
                var buffer = new Byte[100];
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    totalBytesRead += bytesRead;
                }

                console.WrapLine($"{totalBytesRead} read.");
            }

        }
    }
}

