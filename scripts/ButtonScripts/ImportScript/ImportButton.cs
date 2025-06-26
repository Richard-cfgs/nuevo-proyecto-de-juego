using Godot;
using System;

public partial class ImportButton : Button
{
	// Rutas de los nodos
	private const string FileNameInputRoute = "/root/Main/Principal/Panel/Left/ImportButton/LineEdit";
	private const string CodeEditRoute = "/root/Main/Principal/Right/VBoxContainer/CodeEdit";

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
			PixelWallE.Core.Parser.linkedList.Add(("Nombre de archivo vacío.", 0));
			return;
		}

		string fullPath = $"user://{fileName}.pw";

		if (!Godot.FileAccess.FileExists(fullPath))
		{
			PixelWallE.Core.Parser.linkedList.Add(($"El archivo '{fullPath}' no existe.", 0));
			return;
		}

		using var file = Godot.FileAccess.Open(fullPath, FileAccess.ModeFlags.Read);

		if (file == null)
		{
			PixelWallE.Core.Parser.linkedList.Add(($"No se pudo abrir el archivo: {fullPath}", 0));
			return;
		}

		string content = file.GetAsText();
		_codeEdit.Text = content;

		PixelWallE.Core.Parser.linkedList.Add(($"Archivo '{fileName}.pw' importado correctamente.", 0));
	}
}
