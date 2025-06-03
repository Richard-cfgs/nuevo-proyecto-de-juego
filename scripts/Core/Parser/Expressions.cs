using System;

namespace PixelWallE.Core
{
    //todos los tipos de expresiones
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
            public string Operation { get; }
            public Expression Right { get; }
            public ArithmeticExpression(Expression left, string operation, Expression right)
            {
                Left = left;
                Operation = operation;
                Right = right;
            }
        }
    }
}