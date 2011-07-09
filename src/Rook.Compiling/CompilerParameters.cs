using System;
using System.Collections.Generic;
using System.Reflection;
using Rook.Core;

namespace Rook.Compiling
{
    public class CompilerParameters
    {
        private readonly IEnumerable<Assembly> references;

        public CompilerParameters(params Assembly[] references)
        {
            this.references = references;
        }

        public IEnumerable<Assembly> References { get { return references; } }
        public bool GenerateExecutable { get; set; }
        public bool GenerateInMemory { get; set; }
        public bool IncludeDebugInformation { get; set; }

        public static CompilerParameters ForBasicEvaluation()
        {
            return new CompilerParameters(typeof(Func<>).Assembly,//System.Core
                                          typeof(Prelude).Assembly)
            {
                GenerateExecutable = false,
                GenerateInMemory = true,
                IncludeDebugInformation = false
            };
        }
    }
}
