using Godot;
using PixelWallE.Core;
using System;

public partial class RunButton : Button
{
	private const string CodeRoute = "/root/Main/Principal/Right/VBoxContainer/CodeEdit";
	private const string CanvasRoute = "/root/Main/Principal/Right/Canvas";

	private static CodeEdit _codeEdit;
	private static CanvasScript _canvasScript;

	public override void _Ready()
	{
		Pressed += GetCode;
		_codeEdit = GetNode<CodeEdit>(CodeRoute);
		_canvasScript = GetNode<CanvasScript>(CanvasRoute);
	}

	private void GetCode()
	{
		string code = _codeEdit.Text;
		new Lexer(code);

		// Redibujar el canvas después de interpretar
		_canvasScript.Redraw();
	}
}
