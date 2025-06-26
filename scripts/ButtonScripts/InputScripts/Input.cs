using Godot;
using Godot.Collections;
using PixelWallE.Core;
using System;

public partial class Input : SpinBox
{
    public override void _Ready()
    {
        ValueChanged += OnValueChanged;
    }

    private void OnValueChanged(double value)
    {
        Dictionarys.CanvasSize = (int)value;
        PixelWallE.Core.Parser.linkedList.Add(($"Nuevo tamaño del canvas: {Dictionarys.CanvasSize}", 0));
    }
}