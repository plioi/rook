using System;
using System.Text;
using Microsoft.CSharp;
using NUnit.Framework;

namespace Rook.Compiling
{
    [TestFixture]
    public class CSharpCompilerTests : CompilerTests<CSharpCompiler>
    {
        protected override CSharpCompiler Compiler
        {
            get
            {
                var parameters = new CompilerParameters(typeof (Func<>).Assembly, typeof (CSharpCodeProvider).Assembly)
                {
                    GenerateExecutable = true,
                    GenerateInMemory = true,
                    IncludeDebugInformation = false
                };

                return new CSharpCompiler(parameters);
            }
        }

        [Test]
        public void ShouldBuildAssembliesFromCsharpCode()
        {
            Build(ValidProgram);
            AssertErrors(0);
            ExecuteMain().ShouldEqual(123);
        }

        [Test]
        public void ShouldReportErrors()
        {
            Build(InvalidProgram);
            AssertErrors(2);
            AssertError(3, 25, "'Program.Main()' has the wrong signature to be an entry point");
        }

        private static string ValidProgram
        {
            get
            {
                return new StringBuilder()
                    .AppendLine("using System;")
                    .AppendLine("using Microsoft.CSharp;")
                    .AppendLine("public class Program")
                    .AppendLine("{")
                    .AppendLine("   public static int Main()")
                    .AppendLine("   {")
                    .AppendLine("      Func<int, int> f = x => x*2;")
                    .AppendLine("      CSharpCodeProvider c = new CSharpCodeProvider();")
                    .AppendLine("      return 123;")
                    .AppendLine("      ")
                    .AppendLine("   }")
                    .AppendLine("}")
                    .ToString();
            }
        }

        private static string InvalidProgram
        {
            get
            {
                return new StringBuilder()
                    .AppendLine("public class Program")
                    .AppendLine("{")
                    .AppendLine("   public static string Main()")
                    .AppendLine("   {")
                    .AppendLine("      return \"ABC\";")
                    .AppendLine("      ")
                    .AppendLine("   }")
                    .AppendLine("}")
                    .ToString();
            }
        }
    }
}