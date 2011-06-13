using System;
using System.Collections.Generic;
using System.Linq;

namespace Parsley
{
    public class ErrorMessageList
    {
        public static readonly ErrorMessageList Empty = new ErrorMessageList();

        private readonly ErrorMessage head;
        private readonly ErrorMessageList tail;

        private ErrorMessageList()
        {
            head = null;
            tail = null;
        }

        private ErrorMessageList(ErrorMessage head, ErrorMessageList tail)
        {
            this.head = head;
            this.tail = tail;
        }

        public ErrorMessageList With(ErrorMessage errorMessage)
        {
            return new ErrorMessageList(errorMessage, this);
        }

        public ErrorMessageList Merge(ErrorMessageList errors)
        {
            var result = this;

            foreach (var error in errors.All())
                result = result.With(error);

            return result;
        }

        public override string ToString()
        {
            if (this == Empty)
                return "";

            var errors = new List<string>(All()
                                              .Where(error => error.Expectation != null)
                                              .Select(error => error.Expectation)
                                              .Distinct()
                                              .OrderBy(expectation => expectation));

            if (errors.Count == 0)
                return "Parse error.";

            var suffixes = Separators(errors.Count - 1).Concat(new[] {" expected"});

            return String.Join("", errors.Zip(suffixes, (error, suffix) => error + suffix));
        }

        private static IEnumerable<string> Separators(int count)
        {
            if (count <= 0)
                return Enumerable.Empty<string>();
            return Enumerable.Repeat(", ", count - 1).Concat(new[] { " or " });
        }

        private IEnumerable<ErrorMessage> All()
        {
            if (this != Empty)
            {
                
                yield return head;
                foreach (ErrorMessage message in tail.All())
                    yield return message;
            }
        }
    }
}