using Godot;
using System;

public partial class Main : Node2D
{
	private GameManager _gameManager;
	
	// Character scene paths
	private const string GATOT_SCENE = "res://Character Assets/Gatot/Gatot.tscn";
	private const string GILGAMESH_SCENE = "res://Character Assets/Gilgamesh/Gilgamesh.tscn";
	
	// Spawn positions (relative to Main root at (0,0), arena center X=0)
	// Arena floor top at global Y ≈ 241.22. Character collision bottom at root.Y - 24.5.
	// To stand on floor: root.Y = 241.22 + 24.5 = 265.72
	private const float P1_SPAWN_X = 150f;   // Right of center
	private const float P2_SPAWN_X = -150f;  // Left of center
	private const float SPAWN_Y = 265.72f;

	public override void _Ready()
	{
		_gameManager = GetNode<GameManager>("/root/GameManager");
		GD.Print($"Main: Spawning P1={_gameManager.P1Character} at RIGHT, P2={_gameManager.P2Character} at LEFT");
		
		// Diagnose arena structure
		DiagnoseArena();
		
		// Spawn characters based on Character Select choices
		SpawnCharacters();
	}

	private void DiagnoseArena()
	{
		var colloseum = GetNode<Node2D>("Colloseum");
		GD.Print($"Main: Colloseum Position={colloseum.Position}, GlobalPosition={colloseum.GlobalPosition}, Scale={colloseum.Scale}");
		
		var staticBody = colloseum.GetNode<Node2D>("StaticBody2D");
		GD.Print($"Main: StaticBody2D Position={staticBody.Position}, GlobalPosition={staticBody.GlobalPosition}, Scale={staticBody.Scale}");
		
		var floorShape = staticBody.GetNode<CollisionShape2D>("CollisionShape2D");
		var floorRect = floorShape.Shape as RectangleShape2D;
		GD.Print($"Main: Floor CollisionShape2D Position={floorShape.Position}, GlobalPosition={floorShape.GlobalPosition}, Size={floorRect.Size}");
		
		// Calculate floor top in global coordinates
		Vector2 floorShapeGlobalPos = floorShape.GlobalPosition;
		float floorTopGlobalY = floorShapeGlobalPos.Y - floorRect.Size.Y / 2;
		GD.Print($"Main: Floor top global Y = {floorTopGlobalY}");
		
		// Calculate character collision bottom offset
		var p1TestScene = GD.Load<PackedScene>(GATOT_SCENE);
		var p1TestInstance = (CharacterBody2D)p1TestScene.Instantiate();
		var charCollision = p1TestInstance.GetNode<CollisionShape2D>("CollisionShape2D");
		var charRect = charCollision.Shape as RectangleShape2D;
		GD.Print($"Main: Char CollisionShape2D Position={charCollision.Position}, Size={charRect.Size}");
		float charCollisionBottomOffset = charCollision.Position.Y + charRect.Size.Y / 2;
		GD.Print($"Main: Char collision bottom offset from root = {charCollisionBottomOffset}");
		p1TestInstance.QueueFree();
	}

	private void SpawnCharacters()
	{
		// Get selected characters
		var p1Char = _gameManager.P1Character;
		var p2Char = _gameManager.P2Character;
		
		// Spawn P1 (Controller 0) on RIGHT side, facing LEFT
		var p1Scene = p1Char == GameManager.Character.Gatot ? GATOT_SCENE : GILGAMESH_SCENE;
		var p1PackedScene = GD.Load<PackedScene>(p1Scene);
		if (p1PackedScene == null)
		{
			GD.PrintErr($"Main: FAILED to load P1 scene: {p1Scene}");
			return;
		}
		var p1Instance = (CharacterBody2D)p1PackedScene.Instantiate();
		if (p1Instance == null)
		{
			GD.PrintErr("Main: FAILED to instantiate P1");
			return;
		}
		p1Instance.Name = "P1";
		p1Instance.Position = new Vector2(P1_SPAWN_X, SPAWN_Y);
		
		// Set ControllerId to 0 for P1
		var p1Input = p1Instance.GetNode<PlayerInput>("PlayerInput");
		if (p1Input != null)
		{
			p1Input.ControllerId = 0;
		}
		else
		{
			GD.PrintErr("Main: P1 PlayerInput NOT FOUND!");
		}
		
		// Face LEFT (towards P2) - FlipH = true means facing left
		var p1Sprite = p1Instance.GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		if (p1Sprite != null)
		{
			p1Sprite.FlipH = true;
		}
		else
		{
			GD.PrintErr("Main: P1 AnimatedSprite2D NOT FOUND!");
		}
		
		AddChild(p1Instance);
		GD.Print($"Main: P1 after AddChild - Position={p1Instance.Position}, GlobalPosition={p1Instance.GlobalPosition}");
		
		// Spawn P2 (Controller 1) on LEFT side, facing RIGHT
		var p2Scene = p2Char == GameManager.Character.Gatot ? GATOT_SCENE : GILGAMESH_SCENE;
		var p2PackedScene = GD.Load<PackedScene>(p2Scene);
		if (p2PackedScene == null)
		{
			GD.PrintErr($"Main: FAILED to load P2 scene: {p2Scene}");
			return;
		}
		var p2Instance = (CharacterBody2D)p2PackedScene.Instantiate();
		if (p2Instance == null)
		{
			GD.PrintErr("Main: FAILED to instantiate P2");
			return;
		}
		p2Instance.Name = "P2";
		p2Instance.Position = new Vector2(P2_SPAWN_X, SPAWN_Y);
		
		// Set ControllerId to 1 for P2
		var p2Input = p2Instance.GetNode<PlayerInput>("PlayerInput");
		if (p2Input != null)
		{
			p2Input.ControllerId = 1;
		}
		else
		{
			GD.PrintErr("Main: P2 PlayerInput NOT FOUND!");
		}
		
		// Face RIGHT (towards P1) - FlipH = false means facing right
		var p2Sprite = p2Instance.GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		if (p2Sprite != null)
		{
			p2Sprite.FlipH = false;
		}
		else
		{
			GD.PrintErr("Main: P2 AnimatedSprite2D NOT FOUND!");
		}
		
		AddChild(p2Instance);
		GD.Print($"Main: P2 after AddChild - Position={p2Instance.Position}, GlobalPosition={p2Instance.GlobalPosition}");
		
		GD.Print($"Main: Characters spawned - P1({p1Instance.Name}) at global {p1Instance.GlobalPosition}, P2({p2Instance.Name}) at global {p2Instance.GlobalPosition}");
	}
}