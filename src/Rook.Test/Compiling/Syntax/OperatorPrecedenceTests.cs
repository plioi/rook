using Xunit;

namespace Rook.Compiling.Syntax
{
    public class OperatorPrecedenceTests : ExpressionTests
    {
        [Fact]
        public void RanksFunctionCallsBeforeUnaryOperators()
        {
            Parses("-Square(2)").IntoTree("(-((Square(2))))");
            Parses("!Even(3)").IntoTree("(!((Even(3))))");

            Parses("-(GetCallable())(2, 3)").IntoTree("(-(((GetCallable())(2, 3))))");
            Parses("!(GetCallable())(3)").IntoTree("(!(((GetCallable())(3))))");
        }

        [Fact]
        public void RanksMethodInvocationBeforeUnaryOperators()
        {
            Parses("-x.Square()").IntoTree("(-(x.Square()))");
            Parses("!x.Even()").IntoTree("(!(x.Even()))");
        }

        [Fact]
        public void RanksUnaryBeforeMultiplicativeOperators()
        {
            Parses("2*-3").IntoTree("((2) * ((-(3))))");
            Parses("-2*3").IntoTree("(((-(2))) * (3))");
            Parses("false*!true").IntoTree("((false) * ((!(true))))");
            Parses("!false*true").IntoTree("(((!(false))) * (true))");
        }

        [Fact]
        public void RanksMultiplicativeBeforeAdditiveOperators()
        {
            Parses("1+2*3").IntoTree("((1) + (((2) * (3))))");
            Parses("1/2+3").IntoTree("((((1) / (2))) + (3))");
        }

        [Fact]
        public void RanksAdditiveBeforeRelationalOperators()
        {
            Parses("1<2+3").IntoTree("((1) < (((2) + (3))))");
            Parses("1-2>3").IntoTree("((((1) - (2))) > (3))");
            Parses("1<=2+3").IntoTree("((1) <= (((2) + (3))))");
            Parses("1-2>=3").IntoTree("((((1) - (2))) >= (3))");
        }

        [Fact]
        public void RanksRelationalBeforeEqualityOperators()
        {
            Parses("true==2<3").IntoTree("((true) == (((2) < (3))))");
            Parses("1>2!=false").IntoTree("((((1) > (2))) != (false))");
        }

        [Fact]
        public void RanksEqualityBeforeLogicalAndOperator()
        {
            Parses("true&&2==3").IntoTree("((true) && (((2) == (3))))");
            Parses("true&&2!=3").IntoTree("((true) && (((2) != (3))))");
        }

        [Fact]
        public void RanksLogicalAndBeforeLogicalOrOperator()
        {
            Parses("true||false&&false").IntoTree("((true) || (((false) && (false))))");
            Parses("true&&false||false").IntoTree("((((true) && (false))) || (false))");
        }

        [Fact]
        public void RanksLogicalOrBeforeNullCoalescingOperator()
        {
            Parses("null??true||false").IntoTree("((null) ?? (((true) || (false))))");
            Parses("true||null??false").IntoTree("((((true) || (null))) ?? (false))");
        }
    }
}