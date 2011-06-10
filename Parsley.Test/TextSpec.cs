using System.Text;
using NUnit.Framework;

namespace Parsley
{
    [TestFixture]
    public sealed class TextSpec
    {
        [Test]
        public void CanPeekAheadNCharacters()
        {
            var empty = new Text("");
            empty.Peek(0).ShouldEqual("");
            empty.Peek(1).ShouldEqual("");

            var abc = new Text("abc");
            abc.Peek(0).ShouldEqual("");
            abc.Peek(1).ShouldEqual("a");
            abc.Peek(2).ShouldEqual("ab");
            abc.Peek(3).ShouldEqual("abc");
            abc.Peek(4).ShouldEqual("abc");
            abc.Peek(100).ShouldEqual("abc");
        }

        [Test]
        public void CanAdvanceAheadNCharacters()
        {
            var empty = new Text("");
            empty.Advance(0).ToString().ShouldEqual("");
            empty.Advance(1).ToString().ShouldEqual("");

            var abc = new Text("abc");
            abc.Advance(0).ToString().ShouldEqual("abc");
            abc.Advance(1).ToString().ShouldEqual("bc");
            abc.Advance(2).ToString().ShouldEqual("c");
            abc.Advance(3).ToString().ShouldEqual("");
            abc.Advance(4).ToString().ShouldEqual("");
            abc.Advance(100).ToString().ShouldEqual("");
        }

        [Test]
        public void DetectsTheEndOfInput()
        {
            new Text("!").EndOfInput.ShouldBeFalse();
            new Text("").EndOfInput.ShouldBeTrue();
        }

        [Test]
        public void CanMatchLeadingCharactersByPattern()
        {
            const string letters = @"[a-z]+";
            const string digits = @"[0-9]+";
            const string alphanumerics = @"[a-z0-9]+";

            var empty = new Text("");
            empty.Match(new Pattern(letters)).Success.ShouldBeFalse();

            var abc123 = new Text("abc123");
            abc123.Match(new Pattern(digits)).Success.ShouldBeFalse();
            abc123.Match(new Pattern(letters)).Value.ShouldEqual("abc");
            abc123.Match(new Pattern(alphanumerics)).Value.ShouldEqual("abc123");

            abc123.Advance(2).Match(new Pattern(digits)).Success.ShouldBeFalse();
            abc123.Advance(2).Match(new Pattern(letters)).Value.ShouldEqual("c");
            abc123.Advance(2).Match(new Pattern(alphanumerics)).Value.ShouldEqual("c123");

            abc123.Advance(3).Match(new Pattern(digits)).Value.ShouldEqual("123");
            abc123.Advance(3).Match(new Pattern(letters)).Success.ShouldBeFalse();
            abc123.Advance(3).Match(new Pattern(alphanumerics)).Value.ShouldEqual("123");

            abc123.Advance(6).Match(new Pattern(digits)).Success.ShouldBeFalse();
            abc123.Advance(6).Match(new Pattern(letters)).Success.ShouldBeFalse();
            abc123.Advance(6).Match(new Pattern(alphanumerics)).Success.ShouldBeFalse();
        }

        [Test]
        public void CanGetCurrentLineNumber()
        {
            var empty = new Text("");
            empty.Advance(0).Position.Line.ShouldEqual(1);
            empty.Advance(1).Position.Line.ShouldEqual(1);

            var lines = new StringBuilder()
                .AppendLine("Line 1")//Index 0-5, \r\n
                .AppendLine("Line 2")//Index 8-13, \r\n
                .AppendLine("Line 3");//Index 16-21, \r\n
            var list = new Text(lines.ToString());

            list.Advance(0).Position.Line.ShouldEqual(1);
            list.Advance(5).Position.Line.ShouldEqual(1);
            list.Advance(7).Position.Line.ShouldEqual(1);

            list.Advance(8).Position.Line.ShouldEqual(2);
            list.Advance(13).Position.Line.ShouldEqual(2);
            list.Advance(15).Position.Line.ShouldEqual(2);

            list.Advance(16).Position.Line.ShouldEqual(3);
            list.Advance(21).Position.Line.ShouldEqual(3);
            list.Advance(23).Position.Line.ShouldEqual(3);

            list.Advance(24).Position.Line.ShouldEqual(4);
            list.Advance(1000).Position.Line.ShouldEqual(4);
        }

        [Test]
        public void CanGetCurrentColumnNumber()
        {
            var empty = new Text("");
            empty.Advance(0).Position.Column.ShouldEqual(1);
            empty.Advance(1).Position.Column.ShouldEqual(1);

            var lines = new StringBuilder()
                .AppendLine("Line 1")//Index 0-5, \r\n
                .AppendLine("Line 2")//Index 8-13, \r\n
                .AppendLine("Line 3");//Index 16-21, \r\n
            var list = new Text(lines.ToString());

            list.Advance(0).Position.Column.ShouldEqual(1);
            list.Advance(5).Position.Column.ShouldEqual(6);
            list.Advance(7).Position.Column.ShouldEqual(8);

            list.Advance(8).Position.Column.ShouldEqual(1);
            list.Advance(13).Position.Column.ShouldEqual(6);
            list.Advance(15).Position.Column.ShouldEqual(8);

            list.Advance(16).Position.Column.ShouldEqual(1);
            list.Advance(21).Position.Column.ShouldEqual(6);
            list.Advance(23).Position.Column.ShouldEqual(8);

            list.Advance(24).Position.Column.ShouldEqual(1);
            list.Advance(1000).Position.Column.ShouldEqual(1);
        }
    }
}