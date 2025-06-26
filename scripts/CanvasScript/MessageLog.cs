using Godot;
using System;

public partial class MessageLog : RichTextLabel
{
	public override void _Process(double delta)
	{
		string errors = "";
		foreach (var n in PixelWallE.Core.Parser.linkedList)
		{
			errors += n.Item1 + "\n";
		}
		Text = errors;
	}
}
