using System;

namespace PixelWallE.Core
{
    public class Expressions
    {
        public abstract class Expression { }
        public class Number : Expression
        {
            public string Value { get; }
            public Number(string value)
            {
                Value = value;
            }
        }

        public class Variable : Expression
        {
            public string Name { get; }

            public Variable(string name)
            {
                Name = name;
            }
        }
        public class ArithmeticExpression : Expression
        {
            public Expression Left { get; }
            public Expression Right { get; }
            public ArithmeticExpression(Expression left, Expression right)
            {
                Left = left;
                Right = right;
            }
        }
    }
}