using System;
using System.Collections.Generic;
using Godot.NativeInterop;
using PixelWallE.Core;

namespace PixelWallE.Core
{
    public class Dictionarys()
    {
        public static int CanvasSize = 100;
        public static readonly Dictionary<string, int> FunctionSignatures = new()
        {
            { "GetActualX", 0},
            { "GetActualY", 0},
            { "GetCanvasSize", 0},
            { "IsBrushColor", 1},
            { "IsBrushSize", 1},
            { "IsCanvasColor", 3},
            { "GetColorCount", 5}
        };
        public static readonly Dictionary<string, int> InstructionSignatures = new()
        {
            {"Spawn", 2},
            {"Color", 1},
            {"Size", 1},
            {"DrawLine", 3},
            {"DrawCircle", 3},
            {"DrawRectangle", 5},
            {"Fill", 0}
        };
    }
}