using NUnit.Framework;

namespace Parsley
{
    [TestFixture]
    public class ErrorMessageListSpec
    {
        [Test]
        public void ShouldProvideSharedEmptyInstance()
        {
            ErrorMessageList.Empty.ShouldBeTheSameAs(ErrorMessageList.Empty);
        }

        [Test]
        public void CanBeEmpty()
        {
            ErrorMessageList.Empty.ToString().ShouldEqual("");
        }

        [Test]
        public void CreatesNewCollectionWhenAddingItems()
        {
            ErrorMessageList list = ErrorMessageList.Empty.With(new ErrorMessage("expectation"));

            list.ToString().ShouldEqual("expectation expected");
            list.ShouldNotBeTheSameAs(ErrorMessageList.Empty);
        }

        [Test]
        public void CanIncludeMultipleExpectations()
        {
            ErrorMessageList.Empty
                .With(new ErrorMessage("A"))
                .With(new ErrorMessage("B"))
                .ToString().ShouldEqual("A or B expected");

            ErrorMessageList.Empty
                .With(new ErrorMessage("A"))
                .With(new ErrorMessage("B"))
                .With(new ErrorMessage("C"))
                .ToString().ShouldEqual("A, B or C expected");

            ErrorMessageList.Empty
                .With(new ErrorMessage("A"))
                .With(new ErrorMessage("B"))
                .With(new ErrorMessage("C"))
                .With(new ErrorMessage("D"))
                .ToString().ShouldEqual("A, B, C or D expected");
        }

        [Test]
        public void CanIncludeUnknownErrors()
        {
            ErrorMessageList.Empty
                .With(new ErrorMessage())
                .ToString().ShouldEqual("Parse error.");
        }

        [Test]
        public void OmitsEmptyExpectationsFromExpectationLists()
        {
            ErrorMessageList.Empty
                .With(new ErrorMessage("A"))
                .With(new ErrorMessage("B"))
                .With(new ErrorMessage())
                .With(new ErrorMessage("C"))
                .ToString().ShouldEqual("A, B or C expected");
        }

        [Test]
        public void OmitsDuplicateExpectationsFromExpectationLists()
        {
            ErrorMessageList.Empty
                .With(new ErrorMessage("A"))
                .With(new ErrorMessage("A"))
                .With(new ErrorMessage("B"))
                .With(new ErrorMessage("C"))
                .With(new ErrorMessage())
                .With(new ErrorMessage("C"))
                .With(new ErrorMessage("C"))
                .With(new ErrorMessage("A"))
                .ToString().ShouldEqual("A, B or C expected");
        }

        [Test]
        public void CanMergeTwoLists()
        {
            var first = ErrorMessageList.Empty
                .With(new ErrorMessage("A"))
                .With(new ErrorMessage("B"))
                .With(new ErrorMessage())
                .With(new ErrorMessage("C"));

            var second = ErrorMessageList.Empty
                .With(new ErrorMessage("D"))
                .With(new ErrorMessage("B"))
                .With(new ErrorMessage())
                .With(new ErrorMessage("E"));

            first.Merge(second)
                .ToString().ShouldEqual("A, B, C, D or E expected");
        }
    }
}