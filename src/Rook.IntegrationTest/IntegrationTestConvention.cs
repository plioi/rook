using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Fixie;
using Fixie.Conventions;

namespace Rook.IntegrationTest
{
    public class IntegrationTestConvention : Convention
    {
        public IntegrationTestConvention()
        {
            Classes
                .Where(type => type == typeof(IntegrationTests));

            Methods
                .Where(method => method.IsVoid());

            Parameters
                .Add<FromSampleRookCodeFiles>();
        }

        class FromSampleRookCodeFiles : ParameterSource
        {
            public IEnumerable<object[]> GetParameters(MethodInfo method)
            {
                return Directory.GetFiles(Directory.GetCurrentDirectory())
                    .Where(file => file.EndsWith(".rook"))
                    .Select(Path.GetFileName)
                    .Select(file => file.Substring(0, file.Length - ".rook".Length))
                    .Select(file => new object[] { file });
            }
        }
    }
}