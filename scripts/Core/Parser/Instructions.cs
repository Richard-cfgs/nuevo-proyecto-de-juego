using System;
using System.Collections.Generic;
using Godot.NativeInterop;
using PixelWallE.Core;
namespace PixelWallE.Core
{
    //todos los tipos de intrucciones
    public class Instructions
    {
        public abstract class Statement
        {
            public int Line;
        }
        public class SpawnCommand : Statement
        {
            public Expressions.Expression X;
            public Expressions.Expression Y;
        }

        public class ColorCommand : Statement
        {
            public string Color;
        }

        public class SizeCommand : Statement
        {
            public Expressions.Expression Size;
        }

        public class DrawLineCommand : Statement
        {
            public Expressions.Expression DirX;
            public Expressions.Expression DirY;
            public Expressions.Expression Distance;
        }

        public class DrawCircleCommand : Statement
        {
            public Expressions.Expression DirX;
            public Expressions.Expression DirY;
            public Expressions.Expression Radius;
        }

        public class DrawRectangleCommand : Statement
        {
            public Expressions.Expression DirX;
            public Expressions.Expression DirY;
            public Expressions.Expression Distance;
            public Expressions.Expression Width;
            public Expressions.Expression Height;
        }

        public class FillCommand : Statement
        {
            // No necesita argumentos
        }

        public class Assignment : Statement
        {
            public string VariableName;
            public Expressions.Expression Value;
        }

        public class GoToCommand : Statement
        {
            public string Label;
            public Expressions.Expression Condition;
        }

        public class LabelDeclaration : Statement
        {
            public string Name;
        }
    }
}

