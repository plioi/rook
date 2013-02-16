using System;
using System.IO;
using System.Linq;
using NDesk.Options;
using Rook.Compiling;
using CompilerParameters = Rook.Compiling.CompilerParameters;

namespace Rook
{
    public class Program
    {
        public static int Main(string[] args)
        {
            try
            {
                string path;
                bool translate;

                if (!TryParseCommandLineArguments(args, out path, out translate))
                    return (int) ExitCode.Failure;

                string translation;
                var result = Build(path, out translation);

                if (result.HasErrors)
                {
                    foreach (var error in result.Errors)
                        Console.WriteLine(error);

                    return (int)ExitCode.Failure;
                }

                if (translate)
                    Console.WriteLine(translation);
                else
                    result.CompiledAssembly.Execute();

                return (int)ExitCode.Success;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return (int)ExitCode.Failure;
            }
        }

        private static CompilerResult Build(string path, out string translation)
        {
            var compiler = new RookCompiler(CompilerParameters.ForBasicEvaluation());

            return compiler.Build(File.ReadAllText(path), out translation);
        }

        private static bool TryParseCommandLineArguments(string[] args, out string path, out bool translate)
        {
            bool t = false;
            bool helped = false;

            var options = new OptionSet
            {
                {"t|translate", x => t = true},
                {
                    "h|?|help", x =>
                    {
                        Help();
                        helped = true;
                    }
                }
            };

            var remainder = options.Parse(args).ToArray();
            translate = t;

            if (remainder.Length != 1)
            {
                if (!helped) Help();
                path = null;
                return false;
            }

            path = remainder.Single();
            if (!File.Exists(path))
            {
                if (!helped) Help();
                Console.WriteLine("File not found: " + path);
                return false;
            }

            return true;
        }

        private static void Help()
        {
            Console.WriteLine("Usage: rook <source-file-path> [-t|translate]");
        }

        private enum ExitCode
        {
            Success = 0,
            Failure = 1
        }
    }
}