using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;
using ConsoleToolkit;
using ConsoleToolkit.CommandLineInterpretation.ConfigurationAttributes;
using ConsoleToolkit.ConsoleIO;

namespace asSpike.Commands
{
    [Command]
    [Description("Expand files from a single XML file.")]
    public class XpCommand
    {
        private IConsoleAdapter _console;
        private IErrorAdapter _error;

        [Positional]
        [Description("The XML file.")]
        public string XmlFile { get; set; }

        [Option("out", "o")]
        public string OutputPath { get; set; }


        [CommandHandler]
        public void Handle(IConsoleAdapter console, IErrorAdapter error)
        {
            if (string.IsNullOrEmpty(OutputPath))
            {
                error.WrapLine("Please specify an output path.".Red());
                Environment.ExitCode = 100;
                return;
            }

            _console = console;
            _error = error;

            try
            {
                var doc = XDocument.Load(XmlFile);

                foreach (var element in doc.Root.Elements())
                {
                    Extract(element);
                }

            }
            catch (Exception e)
            {
                _error.WrapLine("Unable to process file due to error:".Red());
                _error.WrapLine(e.ToString().Red());
            }
        }

        private void Extract(XElement element)
        {
            var entryType = element.Name.LocalName;
            if (entryType == "directory")
            {
                CreateDirectory(element);
            }
            else if (entryType == "file")
            {
                CreateFile(element);
            }
            else
            {
                _error.WrapLine($"Unrecognised entry type: {entryType.White()}".Red());
                Environment.ExitCode = Math.Min(Environment.ExitCode, 50);
            }
        }

        private void CreateFile(XElement element)
        {
            var path = element.Attribute("path");
            if (path == null)
            {
                _error.WrapLine($"No path for file: {element.ToString().White()}".Red());
                Environment.ExitCode = Math.Min(Environment.ExitCode, 50);
                return;
            }

            var fileTypeElement = element.Element("type");
            if (fileTypeElement == null)
            {
                _error.WrapLine($"No type for file: {path.Value.White()}".Red());
                Environment.ExitCode = Math.Min(Environment.ExitCode, 50);
                return;
            }

            var contentsElement = element.Element("contents");
            if (contentsElement == null)
            {
                _error.WrapLine($"No contents for file: {path.Value.White()}".Red());
                Environment.ExitCode = Math.Min(Environment.ExitCode, 50);
                return;
            }

            var fileType = fileTypeElement.Value;
            var filePath = RationalisePath(path);
            var realPath = Path.Combine(OutputPath, filePath);

            if (fileType == "text")
            {
                File.WriteAllText(realPath, contentsElement.Value);
            }
            else if (fileType == "data")
            {
                File.WriteAllBytes(realPath, Convert.FromBase64String(contentsElement.Value));
            }
            else if (fileType == "compressed")
            {
                File.WriteAllBytes(realPath, DecompressFile(contentsElement.Value));
            }
            else
            {
                _error.WrapLine($"Invalid file type \"{fileType.White()}\" for file: {path.Value.White()}".Red());
                Environment.ExitCode = Math.Min(Environment.ExitCode, 50);
            }
        }

        private string RationalisePath(XAttribute path)
        {
            var dirName = path.Value;
            var allLevels = AllLevels(dirName);
            if (allLevels.LastOrDefault() == Path.GetFileName(OutputPath))
            {
                dirName = string.Empty;
                var count = allLevels.Count() - 1;
                if (count > 0)
                    dirName = allLevels.Take(count).Aggregate((p, e) => Path.Combine(e, p));
            }
            return dirName;
        }

        private void CreateDirectory(XElement element)
        {
            var path = element.Attribute("path");
            if (path == null)
            {
                _error.WrapLine($"No path for directory: {element.ToString().White()}".Red());
                Environment.ExitCode = Math.Min(Environment.ExitCode, 50);
                return;
            }

            var dirName = RationalisePath(path);

            var dir = Path.Combine(OutputPath, dirName);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }

        private IEnumerable<string> AllLevels(string path)
        {
            while (!string.IsNullOrEmpty(path))
            {
                yield return Path.GetFileName(path);
                path = Path.GetDirectoryName(path);
            }
        }

        private byte[] DecompressFile(string data)
        {
            using (var o = new MemoryStream())
            using (var s = new MemoryStream(Convert.FromBase64String(data)))
            {
                using (var z = new GZipStream(s, CompressionMode.Decompress))
                {
                    z.CopyTo(o);
                }

                o.Flush();
                var bytes = o.ToArray();
                return bytes;
            }
        }

        private bool IsNonText(char c)
        {
            return char.IsControl(c) && !char.IsWhiteSpace(c);
        }
    }

}