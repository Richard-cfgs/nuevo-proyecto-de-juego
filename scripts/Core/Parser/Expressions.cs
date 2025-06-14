using System;
using System.Collections.Generic;

namespace PixelWallE.Core
{
    //todos los tipos de expresiones
    public class Expressions
    {
        public abstract class Expression
        {
            public int Line;
        }
        public class NumberLiteral : Expression
        {
            public int Value;
        }

        public class VariableReference : Expression
        {
            public string Name;
        }

        public class BinaryExpression : Expression
        {
            public string Operator;
            public Expression Left;
            public Expression Right;
        }

        public class FunctionCall : Expression
        {
            public string FunctionName;
            public List<Expression> Arguments;
        }

    }
}