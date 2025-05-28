using System;
using Godot;

namespace PixelWallE.Core
{
    public class Instructions
    {
        public abstract class Instruction { }
        public class Spawn : Instruction
        {
            public Expressions.Expression X { get; }
            public Expressions.Expression Y { get; }

            public Spawn(Expressions.Expression x, Expressions.Expression y)
            {
                X = x;
                Y = y;
            }
        }

        public class Color : Instruction
        {
            public Expressions.Expression color { get; }

            public Color(Expressions.Expression color)
            {
                this.color = color;
            }
        }

        public class Size : Instruction
        {
            public Expressions.Expression size { get; }

            public Size(Expressions.Expression size)
            {
                this.size = size;
            }
        }

        public class DrawLine : Instruction
        {
            public Expressions.Expression X { get; }
            public Expressions.Expression Y { get; }
            public Expressions.Expression D { get; }

            public DrawLine(Expressions.Expression x, Expressions.Expression y, Expressions.Expression d)
            {
                X = x;
                Y = y;
                D = d;
            }
        }

        public class Assignment : Instruction
        {
            public string VariableName { get; }
            public Expressions.Expression Expression { get; }

            public Assignment(string variableName, Expressions.Expression expression)
            {
                VariableName = variableName;
                Expression = expression;
            }
        }
    }
}

