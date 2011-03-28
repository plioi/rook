using NUnit.Framework;

namespace Rook.Compiling.Syntax
{
    [TestFixture]
    public sealed class OperatorPrecedenceSpec : ExpressionSpec
    {
        [Test]
        public void RanksFunctionCallsBeforeUnaryOperators()
        {
            AssertTree("(-((Square(2))))", "-Square(2)");
            AssertTree("(!((Even(3))))", "!Even(3)");

            AssertTree("(-(((GetCallable())(2, 3))))", "-(GetCallable())(2, 3)");
            AssertTree("(!(((GetCallable())(3))))", "!(GetCallable())(3)");
        }

        [Test]
        public void RanksUnaryBeforeMultiplicativeOperators()
        {
            AssertTree("((2) * ((-(3))))", "2*-3");
            AssertTree("(((-(2))) * (3))", "-2*3");
            AssertTree("((false) * ((!(true))))", "false*!true");
            AssertTree("(((!(false))) * (true))", "!false*true");
        }

        [Test]
        public void RanksMultiplicativeBeforeAdditiveOperators()
        {
            AssertTree("((1) + (((2) * (3))))", "1+2*3");
            AssertTree("((((1) / (2))) + (3))", "1/2+3");
        }

        [Test]
        public void RanksAdditiveBeforeRelationalOperators()
        {
            AssertTree("((1) < (((2) + (3))))", "1<2+3");
            AssertTree("((((1) - (2))) > (3))", "1-2>3");
            AssertTree("((1) <= (((2) + (3))))", "1<=2+3");
            AssertTree("((((1) - (2))) >= (3))", "1-2>=3");
        }

        [Test]
        public void RanksRelationalBeforeEqualityOperators()
        {
            AssertTree("((true) == (((2) < (3))))", "true==2<3");
            AssertTree("((((1) > (2))) != (false))", "1>2!=false");
        }

        [Test]
        public void RanksEqualityBeforeLogicalAndOperator()
        {
            AssertTree("((true) && (((2) == (3))))", "true&&2==3");
            AssertTree("((true) && (((2) != (3))))", "true&&2!=3");
        }

        [Test]
        public void RanksLogicalAndBeforeLogicalOrOperator()
        {
            AssertTree("((true) || (((false) && (false))))", "true||false&&false");
            AssertTree("((((true) && (false))) || (false))", "true&&false||false");
        }

        [Test]
        public void RanksLogicalOrBeforeNullCoalescingOperator()
        {
            AssertTree("((null) ?? (((true) || (false))))", "null??true||false");
            AssertTree("((((true) || (null))) ?? (false))", "true||null??false");
        }
    }
}