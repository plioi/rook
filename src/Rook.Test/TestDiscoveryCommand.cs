using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Sdk;

namespace Rook
{
    public abstract class TestDiscoveryCommand : ITestClassCommand
    {
        private readonly TestClassCommand defaultBehavior = new TestClassCommand();

        public abstract bool IsTestMethod(IMethodInfo testMethod);

        public abstract IEnumerable<ITestCommand> EnumerateTestCommands(IMethodInfo testMethod);

        public IEnumerable<IMethodInfo> EnumerateTestMethods()
        {
            return TypeUnderTest.GetMethods().Where(IsTestMethod);
        }

        public object ObjectUnderTest
        {
            get { return defaultBehavior.ObjectUnderTest; }
        }

        public ITypeInfo TypeUnderTest
        {
            get { return defaultBehavior.TypeUnderTest; }
            set { defaultBehavior.TypeUnderTest = value; }
        }

        public int ChooseNextTest(ICollection<IMethodInfo> testsLeftToRun)
        {
            return defaultBehavior.ChooseNextTest(testsLeftToRun);
        }

        public Exception ClassStart()
        {
            try
            {
                foreach (var @interface in TypeUnderTest.Type.GetInterfaces())
                    if (@interface.IsGenericType && @interface.GetGenericTypeDefinition() == typeof(IUseFixture<>))
                        throw new NotSupportedException(GetType() + "does not support IUseFixture<>.");

                return null;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        public Exception ClassFinish()
        {
            return null;
        }
    }
}