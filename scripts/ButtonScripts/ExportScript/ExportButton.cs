using Godot;
using System;

public partial class ExportButton : Button
{
	private const string FileNameInputRoute = "/root/Main/Principal/Panel/Left/ExportButton/FileNameInputRoute";

	private const string CodeEditRoute = "/root/Main/Principal/Right/VBoxContainer/CodeEdit";

	private LineEdit _fileNameInput;
	private CodeEdit _codeEdit;

	public override void _Ready()
	{
		_fileNameInput = GetNode<LineEdit>(FileNameInputRoute);
		_codeEdit = GetNode<CodeEdit>(CodeEditRoute);

		Pressed += ExportCode;
	}

	private void ExportCode()
	{
		string rawName = _fileNameInput.Text.StripEdges();

		if (string.IsNullOrEmpty(rawName))
		{
			GD.PrintErr("Por favor, ingresá un nombre para el archivo.");
			return;
		}

		string fileName = $"user://{rawName}.pw";
		string code = _codeEdit.Text;

		using var f = Godot.FileAccess.Open(fileName, Godot.FileAccess.ModeFlags.Write);

		if (f == null)
		{
			GD.PrintErr($"No se pudo abrir el archivo para escribir: {fileName}");
			return;
		}

		f.StoreString(code);
		GD.Print($"Código exportado exitosamente a: {fileName}");
	}
}
