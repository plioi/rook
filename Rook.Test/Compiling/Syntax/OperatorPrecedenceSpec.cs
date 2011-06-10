using NUnit.Framework;

namespace Rook.Compiling.Syntax
{
    [TestFixture]
    public class OperatorPrecedenceSpec : ExpressionSpec
    {
        [Test]
        public void RanksFunctionCallsBeforeUnaryOperators()
        {
            Parses("-Square(2)").IntoTree("(-((Square(2))))");
            Parses("!Even(3)").IntoTree("(!((Even(3))))");

            Parses("-(GetCallable())(2, 3)").IntoTree("(-(((GetCallable())(2, 3))))");
            Parses("!(GetCallable())(3)").IntoTree("(!(((GetCallable())(3))))");
        }

        [Test]
        public void RanksUnaryBeforeMultiplicativeOperators()
        {
            Parses("2*-3").IntoTree("((2) * ((-(3))))");
            Parses("-2*3").IntoTree("(((-(2))) * (3))");
            Parses("false*!true").IntoTree("((false) * ((!(true))))");
            Parses("!false*true").IntoTree("(((!(false))) * (true))");
        }

        [Test]
        public void RanksMultiplicativeBeforeAdditiveOperators()
        {
            Parses("1+2*3").IntoTree("((1) + (((2) * (3))))");
            Parses("1/2+3").IntoTree("((((1) / (2))) + (3))");
        }

        [Test]
        public void RanksAdditiveBeforeRelationalOperators()
        {
            Parses("1<2+3").IntoTree("((1) < (((2) + (3))))");
            Parses("1-2>3").IntoTree("((((1) - (2))) > (3))");
            Parses("1<=2+3").IntoTree("((1) <= (((2) + (3))))");
            Parses("1-2>=3").IntoTree("((((1) - (2))) >= (3))");
        }

        [Test]
        public void RanksRelationalBeforeEqualityOperators()
        {
            Parses("true==2<3").IntoTree("((true) == (((2) < (3))))");
            Parses("1>2!=false").IntoTree("((((1) > (2))) != (false))");
        }

        [Test]
        public void RanksEqualityBeforeLogicalAndOperator()
        {
            Parses("true&&2==3").IntoTree("((true) && (((2) == (3))))");
            Parses("true&&2!=3").IntoTree("((true) && (((2) != (3))))");
        }

        [Test]
        public void RanksLogicalAndBeforeLogicalOrOperator()
        {
            Parses("true||false&&false").IntoTree("((true) || (((false) && (false))))");
            Parses("true&&false||false").IntoTree("((((true) && (false))) || (false))");
        }

        [Test]
        public void RanksLogicalOrBeforeNullCoalescingOperator()
        {
            Parses("null??true||false").IntoTree("((null) ?? (((true) || (false))))");
            Parses("true||null??false").IntoTree("((((true) || (null))) ?? (false))");
        }
    }
}