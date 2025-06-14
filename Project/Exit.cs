using Godot;
using System;

public partial class Exit : Button
{
	public override void _Ready()
	{
		Pressed += ExitButton;
	}

	private static void ExitButton()
	{
		System.Environment.Exit(0);
	}
}
