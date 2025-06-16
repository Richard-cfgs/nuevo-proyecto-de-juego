using Godot;
using System;
using System.Collections.Generic;

public partial class CanvasScript : Control
{
	// Configuración
	private int _pixelSize = 5;
	private Dictionary<string, Color> _colors = new Dictionary<string, Color>()
	{
		{"White", Colors.White},
		{"Black", Colors.Black},
		{"Red", Colors.Red},
		{"Blue", Colors.Blue},
		{"Green", Colors.Green},
		{"Transparent", Colors.Transparent}
	};
	private string[,] _pixelData;

	public override void _Ready()
	{
		InitializeCanvas(100, 100); // Tamaño inicial
	}

	private void InitializeCanvas(int width, int height)
	{
		_pixelData = new string[height, width];
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				_pixelData[y, x] = "White";
			}
		}
		QueueRedraw();
	}

	public override void _Draw()
	{
		for (int y = 0; y < _pixelData.GetLength(0); y++)
		{
			for (int x = 0; x < _pixelData.GetLength(1); x++)
			{
				string colorName = _pixelData[y, x];
				if (colorName != "Transparent" && _colors.TryGetValue(colorName, out Color color))
				{
					DrawRect(new Rect2(x * _pixelSize, y * _pixelSize, _pixelSize, _pixelSize), color);
				}
			}
		}
	}

	// Llamado desde tu Interprete.cs
	public void UpdatePixels(string[,] newPixels)
	{
		if (newPixels != null && newPixels.Length > 0)
		{
			_pixelData = newPixels;
			QueueRedraw();
		}
	}
}
