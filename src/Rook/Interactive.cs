using System;
using System.Collections;
using System.Linq;
using System.Text;
using Rook.Compiling;
using Rook.Core.Collections;

namespace Rook
{
    public class Interactive
    {
        private readonly Interpreter interpreter;

        public Interactive()
        {
            interpreter = new Interpreter();
        }

        public void Run()
        {
            while (true)
            {
                var firstLine = PromptStart();

                if (firstLine == "exit")
                    return;

                if (firstLine == "translate")
                    TranslateFunctions();
                else
                    OutputResults(interpreter.Interpret(LinesToInterpret(firstLine)));
            }
        }

        private string LinesToInterpret(string firstLine)
        {
            var code = new StringBuilder();
            code.AppendLine(firstLine);

            while (!interpreter.CanParse(code.ToString()))
            {
                var line = PromptMore();

                if (line == "")
                    break;

                code.AppendLine(line);
            }

            return code.ToString();
        }

        private static void OutputResults(InterpreterResult result)
        {
            if (!result.Errors.Any() && result.Value != Core.Void.Value)
            {
                if (result.Value == null)
                {
                    Console.WriteLine("null");
                }
                else if (result.Value is IEnumerable && !(result.Value is string))
                {
                    string commaSeparated = String.Join(", ", ((IEnumerable) result.Value).Cast<object>().ToArray());

                    if (IsSubclassOfRawGeneric(result.Value, typeof(Vector<>)))
                        Console.WriteLine("[{0}]", commaSeparated);
                    else
                        Console.WriteLine(commaSeparated);
                }
                else
                    Console.WriteLine(result.Value);
            }

            var language = result.Language == Language.Rook ? "" : String.Format("({0}) ", result.Language);

            foreach (var error in result.Errors)
                Console.WriteLine(language + error);

            Console.WriteLine();
        }

        private void TranslateFunctions()
        {
            Console.WriteLine(interpreter.Translate());
        }

        private static string PromptStart()
        {
            Console.Write("rook> ");
            return Console.ReadLine();
        }

        private static string PromptMore()
        {
            Console.Write("    > ");
            return Console.ReadLine();
        }

        private static bool IsSubclassOfRawGeneric(object o, Type generic)
        {
            Type toCheck = o.GetType();

            while (toCheck != typeof(object))
            {
                var candidate = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;

                if (generic == candidate)
                    return true;

                toCheck = toCheck.BaseType;
            }

            return false;
        }
    }
}