using Godot;
using PixelWallE.Core;
using System;

public partial class RunButton : Button
{
	private const string Route = "/root/Main/Principal/Right/CodeEdit";
	private static CodeEdit _codeEdit;

	public override void _Ready()
	{
		//Conectar señal "Pressed" al método GetCode
		Pressed += GetCode;

		//Obtener referencia al CodeEdit
		_codeEdit = GetNode<CodeEdit>(Route);
	}

	private static void GetCode()
	{
		//Obtener texto del CodeEdit
		string code = _codeEdit.Text;
		//LLamar a la clase Lexer
		new Lexer(code);
	}
}
