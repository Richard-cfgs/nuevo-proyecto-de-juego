using Godot;
using System;
using PixelWallE.Core;

public partial class CanvasScript : Control
{
    public override void _Draw()
    {
        int canvasSize = Dictionarys.CanvasSize;
        float cellWidth = Size.X / canvasSize;
        float cellHeight = Size.Y / canvasSize;

        for (int y = 0; y < canvasSize; y++)
        {
            for (int x = 0; x < canvasSize; x++)
            {
                string colorName = Canvas.GetPixel(x, y);
                Color drawColor = ColorFromName(colorName);

                Rect2 rect = new Rect2(x * cellWidth, y * cellHeight, cellWidth, cellHeight);
                DrawRect(rect, drawColor);
            }
        }
    }

    public void Redraw()
    {
        QueueRedraw();
    }

    private Color ColorFromName(string name)
    {
        return name switch
        {
            "Red" => new Color(1, 0, 0),
            "Green" => new Color(0, 1, 0),
            "Blue" => new Color(0, 0, 1),
            "Yellow" => new Color(1, 1, 0),
            "Orange" => new Color(1, 0.65f, 0),
            "Purple" => new Color(0.5f, 0, 0.5f),
            "Black" => new Color(0, 0, 0),
            "White" => new Color(1, 1, 1),
            "Transparent" => new Color(0, 0, 0, 0),
            _ => new Color(0.8f, 0.8f, 0.8f) // Gris por defecto
        };
    }

}
