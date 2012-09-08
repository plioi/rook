using System.Collections.Generic;
using Xunit.Sdk;

namespace Rook
{
    public class FactsTestClassCommand : TestDiscoveryCommand
    {
        public override bool IsTestMethod(IMethodInfo testMethod)
        {
            return !testMethod.IsAbstract &&
                   !testMethod.IsStatic &&
                   testMethod.MethodInfo.IsPublic &&
                   testMethod.MethodInfo.ReturnType == typeof(void) &&
                   testMethod.MethodInfo.GetParameters().Length == 0;
        }

        public override IEnumerable<ITestCommand> EnumerateTestCommands(IMethodInfo testMethod)
        {
            yield return new FactCommand(testMethod);
        }
    }
}