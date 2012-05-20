using System;
using System.Text;
using Microsoft.CSharp;
using Should;
using Xunit;

namespace Rook.Compiling
{
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

        [Fact]
        public void ShouldBuildAssembliesFromCsharpCode()
        {
            Build(ValidProgram);
            AssertErrors(0);
            Execute().ShouldEqual(123);
        }

        [Fact]
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