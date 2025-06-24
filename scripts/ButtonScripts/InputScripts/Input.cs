using Godot;
using Godot.Collections;
using PixelWallE.Core;
using System;

public partial class Input : SpinBox
{
    public override void _Ready()
    {
        // Conecta la señal "value_changed" al método OnValueChanged
        ValueChanged += OnValueChanged;
    }

    private void OnValueChanged(double value)
    {
        // Guarda el valor en una variable global
        Dictionarys.CanvasSize = (int)value;
        GD.Print("Nuevo tamaño del canvas: ", Dictionarys.CanvasSize);
    }
}