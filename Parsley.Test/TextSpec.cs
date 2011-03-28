using System;
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
            Text empty = new Text("");
            Assert.AreEqual("", empty.Peek(0));
            Assert.AreEqual("", empty.Peek(1));

            Text abc = new Text("abc");
            Assert.AreEqual("", abc.Peek(0));
            Assert.AreEqual("a", abc.Peek(1));
            Assert.AreEqual("ab", abc.Peek(2));
            Assert.AreEqual("abc", abc.Peek(3));
            Assert.AreEqual("abc", abc.Peek(4));
            Assert.AreEqual("abc", abc.Peek(100));
        }

        [Test]
        public void CanAdvanceAheadNCharacters()
        {
            Text empty = new Text("");
            Assert.AreEqual("", empty.Advance(0).ToString());
            Assert.AreEqual("", empty.Advance(1).ToString());

            Text abc = new Text("abc");
            Assert.AreEqual("abc", abc.Advance(0).ToString());
            Assert.AreEqual("bc", abc.Advance(1).ToString());
            Assert.AreEqual("c", abc.Advance(2).ToString());
            Assert.AreEqual("", abc.Advance(3).ToString());
            Assert.AreEqual("", abc.Advance(4).ToString());
            Assert.AreEqual("", abc.Advance(100).ToString());
        }

        [Test]
        public void DetectsTheEndOfInput()
        {
            Assert.IsFalse(new Text("!").EndOfInput);
            Assert.IsTrue(new Text("").EndOfInput);
        }

        [Test]
        public void CanCountLeadingCharactersSatisfyingAPredicate()
        {
            Text empty = new Text("");
            Assert.AreEqual(0, empty.Count(Char.IsLetter));

            Text abc123 = new Text("abc123");
            Assert.AreEqual(0, abc123.Count(Char.IsDigit));
            Assert.AreEqual(3, abc123.Count(Char.IsLetter));
            Assert.AreEqual(6, abc123.Count(Char.IsLetterOrDigit));

            Assert.AreEqual(0, abc123.Advance(2).Count(Char.IsDigit));
            Assert.AreEqual(1, abc123.Advance(2).Count(Char.IsLetter));
            Assert.AreEqual(4, abc123.Advance(2).Count(Char.IsLetterOrDigit));

            Assert.AreEqual(3, abc123.Advance(3).Count(Char.IsDigit));
            Assert.AreEqual(0, abc123.Advance(3).Count(Char.IsLetter));
            Assert.AreEqual(3, abc123.Advance(3).Count(Char.IsLetterOrDigit));

            Assert.AreEqual(0, abc123.Advance(6).Count(Char.IsDigit));
            Assert.AreEqual(0, abc123.Advance(6).Count(Char.IsLetter));
            Assert.AreEqual(0, abc123.Advance(6).Count(Char.IsLetterOrDigit));
        }

        [Test]
        public void CanGetCurrentLineNumber()
        {
            Text empty = new Text("");
            Assert.AreEqual(1, empty.Advance(0).Line);
            Assert.AreEqual(1, empty.Advance(1).Line);

            StringBuilder lines = new StringBuilder()
                .AppendLine("Line 1")//Index 0-5, \r\n
                .AppendLine("Line 2")//Index 8-13, \r\n
                .AppendLine("Line 3");//Index 16-21, \r\n
            Text list = new Text(lines.ToString());
           
            Assert.AreEqual(1, list.Advance(0).Line);
            Assert.AreEqual(1, list.Advance(5).Line);
            Assert.AreEqual(1, list.Advance(7).Line);

            Assert.AreEqual(2, list.Advance(8).Line);
            Assert.AreEqual(2, list.Advance(13).Line);
            Assert.AreEqual(2, list.Advance(15).Line);

            Assert.AreEqual(3, list.Advance(16).Line);
            Assert.AreEqual(3, list.Advance(21).Line);
            Assert.AreEqual(3, list.Advance(23).Line);

            Assert.AreEqual(4, list.Advance(24).Line);
            Assert.AreEqual(4, list.Advance(1000).Line);
        }

        [Test]
        public void CanGetCurrentColumnNumber()
        {
            Text empty = new Text("");
            Assert.AreEqual(1, empty.Advance(0).Column);
            Assert.AreEqual(1, empty.Advance(1).Column);

            StringBuilder lines = new StringBuilder()
                .AppendLine("Line 1")//Index 0-5, \r\n
                .AppendLine("Line 2")//Index 8-13, \r\n
                .AppendLine("Line 3");//Index 16-21, \r\n
            Text list = new Text(lines.ToString());

            Assert.AreEqual(1, list.Advance(0).Column);
            Assert.AreEqual(6, list.Advance(5).Column);
            Assert.AreEqual(8, list.Advance(7).Column);

            Assert.AreEqual(1, list.Advance(8).Column);
            Assert.AreEqual(6, list.Advance(13).Column);
            Assert.AreEqual(8, list.Advance(15).Column);

            Assert.AreEqual(1, list.Advance(16).Column);
            Assert.AreEqual(6, list.Advance(21).Column);
            Assert.AreEqual(8, list.Advance(23).Column);

            Assert.AreEqual(1, list.Advance(24).Column);
            Assert.AreEqual(1, list.Advance(1000).Column);
        }
    }
}