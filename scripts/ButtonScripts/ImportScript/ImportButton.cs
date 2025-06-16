using Godot;
using System;

public partial class ImportButton : Button
{
	// Rutas de los nodos
	private const string FileNameInputRoute = "/root/Main/Principal/Panel/Left/ImportButton/LineEdit";
	private const string CodeEditRoute = "/root/Main/Principal/Right/VBoxContainer/CodeEditt";

	private LineEdit _fileNameInput;
	private CodeEdit _codeEdit;

	public override void _Ready()
	{
		_fileNameInput = GetNode<LineEdit>(FileNameInputRoute);
		_codeEdit = GetNode<CodeEdit>(CodeEditRoute);

		Pressed += OnImportPressed;
	}

	private void OnImportPressed()
	{
		string fileName = _fileNameInput.Text.StripEdges();
		ImportCode(fileName);
	}

	private void ImportCode(string fileName)
	{
		if (string.IsNullOrEmpty(fileName))
		{
			GD.PrintErr("Nombre de archivo vacío.");
			return;
		}

		string fullPath = $"user://{fileName}.pw";

		if (!Godot.FileAccess.FileExists(fullPath))
		{
			GD.PrintErr($"El archivo '{fullPath}' no existe.");
			return;
		}

		using var file = Godot.FileAccess.Open(fullPath, FileAccess.ModeFlags.Read);

		if (file == null)
		{
			GD.PrintErr($"No se pudo abrir el archivo: {fullPath}");
			return;
		}

		string content = file.GetAsText();
		_codeEdit.Text = content;

		GD.Print($"Archivo '{fileName}.pw' importado correctamente.");
	}
}
