using Godot;
using System;

public partial class GameManager : Node
{
	public enum Character
	{
		Gatot = 0,
		Gilgamesh = 1
	}

	// Selected characters from Character Select
	public Character P1Character { get; private set; } = Character.Gatot;
	public Character P2Character { get; private set; } = Character.Gilgamesh;

	// Spawn positions (relative to Main root at (0,0), arena center X=0)
	// Arena floor top at global Y ≈ 241.22. Character collision bottom at root.Y - 24.5.
	// To stand on floor: root.Y = 241.22 + 24.5 = 265.72
	public const float P1_SPAWN_X = 150f;  // Right of center
	public const float P2_SPAWN_X = -150f; // Left of center
	public const float SPAWN_Y = 265.72f;

	public override void _Ready()
	{
		// Ensure this node persists across scene changes
		ProcessMode = ProcessModeEnum.Always;
	}

	public void SetSelections(Character p1Char, Character p2Char)
	{
		P1Character = p1Char;
		P2Character = p2Char;
		GD.Print($"GameManager: P1 = {P1Character}, P2 = {P2Character}");
	}

	public string GetP1CharacterScene()
	{
		return P1Character == Character.Gatot
			? "res://Character Assets/Gatot/Gatot.tscn"
			: "res://Character Assets/Gilgamesh/Gilgamesh.tscn";
	}

	public string GetP2CharacterScene()
	{
		return P2Character == Character.Gatot
			? "res://Character Assets/Gatot/Gatot.tscn"
			: "res://Character Assets/Gilgamesh/Gilgamesh.tscn";
	}
}