using System;
using System.Collections.Generic;

namespace PixelWallE.Core
{
    public class AST
    {
        public List<Instructions.Instruction> Tree = new List<Instructions.Instruction>();
        public void AddTree(Instructions.Instruction node)
        {
            Tree.Add(node);
        }
    }
}