using System;
using System.Collections.Generic;
using Godot.NativeInterop;
using PixelWallE.Core;

namespace PixelWallE.Core
{
    public class Dictionarys()
    {
        public static readonly Dictionary<string, Expressions.FunctionSignature> FunctionSignatures = new()
        {
            { "GetActualX", new Expressions.FunctionSignature { ExpectedCount = 0, ExpectedTypes = new List<string>() } },
            { "GetActualY", new Expressions.FunctionSignature { ExpectedCount = 0, ExpectedTypes = new List<string>() } },
            { "GetCanvasSize", new Expressions.FunctionSignature { ExpectedCount = 0, ExpectedTypes = new List<string>() } },
            { "IsBrushColor", new Expressions.FunctionSignature { ExpectedCount = 1, ExpectedTypes = new List<string> { "color" } } },
            { "IsBrushSize", new Expressions.FunctionSignature { ExpectedCount = 1, ExpectedTypes = new List<string> { "number" } } },
            { "IsCanvasColor", new Expressions.FunctionSignature { ExpectedCount = 3, ExpectedTypes = new List<string> { "color", "number", "number" } } },
            { "GetColorCount", new Expressions.FunctionSignature { ExpectedCount = 5, ExpectedTypes = new List<string> { "color", "number", "number", "number", "number" } } }
        };
        public static readonly Dictionary<string, Instructions.InstructionSignature> InstructionSignatures = new()
        {
            {
                "Spawn",
                new Instructions.InstructionSignature
                {
                    ExpectedCount = 2,
                    ExpectedTypes = new List<string> { "expression", "expression" }
                }
            },
            {
                "Color",
                new Instructions.InstructionSignature
                {
                    ExpectedCount = 1,
                    ExpectedTypes = new List<string> { "color" }
                }
            },
            {
                "Size",
                new Instructions.InstructionSignature
                {
                    ExpectedCount = 1,
                    ExpectedTypes = new List<string> { "expression" }
                }
            },
            {
                "DrawLine",
                new Instructions.InstructionSignature
                {
                    ExpectedCount = 3,
                    ExpectedTypes = new List<string> { "expression", "expression", "expression" }
                }
            },
            {
                "DrawCircle",
                new Instructions.InstructionSignature
                {
                    ExpectedCount = 3,
                    ExpectedTypes = new List<string> { "expression", "expression", "expression" }
                }
            },
            {
                "DrawRectangle",
                new Instructions.InstructionSignature
                {
                    ExpectedCount = 5,
                    ExpectedTypes = new List<string> { "expression", "expression", "expression", "expression", "expression" }
                }
            },
            {
                "Fill",
                new Instructions.InstructionSignature
                {
                    ExpectedCount = 0,
                    ExpectedTypes = new List<string>()
                }
            }
        };
    }
}