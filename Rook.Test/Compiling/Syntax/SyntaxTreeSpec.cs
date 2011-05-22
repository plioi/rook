using System.Linq;
using Parsley;
using Text = Parsley.Text;
using Rook.Compiling.Types;

namespace Rook.Compiling.Syntax
{
    public abstract class SyntaxTreeSpec<TSyntax> where TSyntax : SyntaxTree
    {
        protected static readonly DataType Integer = NamedType.Integer;
        protected static readonly DataType Boolean = NamedType.Boolean;

        protected TSyntax Parse(string source)
        {
            return ParserUnderTest(new Text(source)).Value;
        }

        protected void AssertTree(string expectedSyntaxTree, string source)
        {
            AssertTree(expectedSyntaxTree, source, new Serializer());
         
            //Although not strictly necessary, we perform the same test using the serialized tree as the input source code.
            //This is simply a sanity check that the serializer is producing code that is equivalent to source.
            //This has also been useful in discovering ambiguity in the grammar.
            Parsed<TSyntax> result = ParserUnderTest(new Text(source));
            string serializedSource = result.Value.Visit(new Serializer());
            AssertTree(expectedSyntaxTree, serializedSource, new Serializer());
        }

        private void AssertTree(string expectedSyntaxTree, string source, Serializer serializer)
        {
            const string expectedUnparsedSource = "";
            ParserUnderTest.AssertParse(source, expectedUnparsedSource,
                                        parsedValue => parsedValue.Visit(serializer).ShouldEqual(expectedSyntaxTree));
            Parse(source).ShouldBeInstanceOf<TSyntax>();
        }

        protected void AssertError(string source, string expectedUnparsedSource)
        {
            ParserUnderTest.AssertError(source, expectedUnparsedSource);
        }

        protected void AssertError(string source, string expectedUnparsedSource, string expectedMessage)
        {
            ParserUnderTest.AssertError(source, expectedUnparsedSource, expectedMessage);
        }

        protected static void AssertTypeCheckError(TypeChecked<TSyntax> typeChecked, int line, int column, string expectedMessage)
        {
            typeChecked.Syntax.ShouldBeNull("Expected type check error but found type checked syntax.");
            typeChecked.HasErrors.ShouldBeTrue();
            
            if (typeChecked.Errors.Count() != 1)
            {
                Fail.WithErrors(typeChecked.Errors, line, column, expectedMessage);
            }
            else
            {
                CompilerError error = typeChecked.Errors.First();

                if (line != error.Line || column != error.Column || expectedMessage != error.Message)
                    Fail.WithErrors(typeChecked.Errors, line, column, expectedMessage);
            }
        }

        protected abstract Parser<TSyntax> ParserUnderTest { get; }

        protected delegate DataType TypeMapping(string name);

        protected static Environment Environment(params TypeMapping[] symbols)
        {
            var rootEnvironment = new Environment();
            var environment = new Environment(rootEnvironment);

            foreach (var symbol in symbols)
            {
                var item = symbol(null);
                var name = symbol.Method.GetParameters()[0].Name;
                environment[name] = item;
            }

            return environment;
        }
    }
}