using System;
using System.Linq;

namespace Parsley
{
    public sealed class Text
    {
        private readonly int index;
        private readonly string source;

        public Text(string source)
            : this(source, 0) {}

        private Text(string source, int index)
        {
            this.source = source;
            this.index = index;

            if (index > source.Length)
                this.index = source.Length;
        }

        public string Peek(int characters)
        {
            if (index + characters >= source.Length)
                return source.Substring(index);

            return source.Substring(index, characters);
        }

        public Text Advance(int characters)
        {
            if (characters == 0)
                return this;

            return new Text(source, index + characters);
        }

        public bool EndOfInput
        {
            get { return index >= source.Length; }
        }

        public int Count(Predicate<char> match)
        {
            int sizeOfMatch = 0;

            while (index + sizeOfMatch < source.Length && match(source[index + sizeOfMatch]))
                sizeOfMatch++;

            return sizeOfMatch;
        }

        public int Line
        {
            get
            {
                const int firstLineNumber = 1;
                return source.Take(index).Count(ch => ch == '\n') + firstLineNumber;
            }
        }

        public int Column
        {
            get
            {
                if (index == 0)
                    return 1;

                int indexOfPreviousNewLine = source.LastIndexOf('\n', index - 1);
                return index - indexOfPreviousNewLine;
            }
        }

        public override string ToString()
        {
            return source.Substring(index);
        }
    }
}