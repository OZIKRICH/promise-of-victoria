using Godot;
using System;

public partial class CharacterSelect : Control
{
	// Use GameManager's Character enum
	private GameManager.Character _p1Selection = GameManager.Character.Gatot;
	private bool _p1Ready = false;

	// Player 2 state
	private GameManager.Character _p2Selection = GameManager.Character.Gatot;
	private bool _p2Ready = false;

	// UI elements
	private TextureRect _p1Portrait;
	private TextureRect _p2Portrait;
	private Label _p1NameLabel;
	private Label _p2NameLabel;
	private Label _p1ReadyLabel;
	private Label _p2ReadyLabel;
	private Label _titleLabel;
	private Label _hintLabel;
	private Label _startLabel;

	// Input cooldowns
	private float _p1AnalogCooldown = 0f;
	private float _p2AnalogCooldown = 0f;
	private float _startCooldown = 0f;
	private const float ANALOG_DEADZONE = 0.3f;
	private const float ANALOG_COOLDOWN_TIME = 0.2f;
	private const float START_COOLDOWN_TIME = 0.5f;

	// Controller IDs
	private const int P1_CONTROLLER_ID = 0;
	private const int P2_CONTROLLER_ID = 1;

	// GameManager reference
	private GameManager _gameManager;

	public override void _Ready()
	{
		// Get GameManager
		_gameManager = GetNode<GameManager>("/root/GameManager");

		// Get UI elements
		_p1Portrait = GetNode<TextureRect>("P1Container/Portrait");
		_p2Portrait = GetNode<TextureRect>("P2Container/Portrait");
		_p1NameLabel = GetNode<Label>("P1Container/InfoContainer/NameLabel");
		_p2NameLabel = GetNode<Label>("P2Container/InfoContainer/NameLabel");
		_p1ReadyLabel = GetNode<Label>("P1Container/InfoContainer/ReadyLabel");
		_p2ReadyLabel = GetNode<Label>("P2Container/InfoContainer/ReadyLabel");
		_titleLabel = GetNode<Label>("TitleLabel");
		_hintLabel = GetNode<Label>("HintLabel");

		// Create START label (initially hidden)
		_startLabel = new Label();
		_startLabel.Text = "PRESS START";
		_startLabel.HorizontalAlignment = HorizontalAlignment.Center;
		_startLabel.VerticalAlignment = VerticalAlignment.Center;
		_startLabel.AddThemeFontSizeOverride("font_size", 32);
		_startLabel.AddThemeColorOverride("font_color", Colors.Yellow);
		_startLabel.CustomMinimumSize = new Vector2(0, 60);
		AddChild(_startLabel);
		_startLabel.Visible = false;

		// Ensure mouse cursor is visible
		Input.MouseMode = Input.MouseModeEnum.Visible;

		// Initial UI update
		UpdateP1Display();
		UpdateP2Display();
	}

	public override void _Process(double delta)
	{
		float dt = (float)delta;
		HandleP1Input(dt);
		HandleP2Input(dt);
		HandleStartInput();
		UpdateStartLabelPosition();
	}

	private void HandleP1Input(float delta)
	{
		if (_p1Ready) return; // P1 can't change selection after ready

		// Left/Right navigation (digital)
		if (Input.IsActionJustPressed("p1_left"))
		{
			NavigateP1Left();
		}
		else if (Input.IsActionJustPressed("p1_right"))
		{
			NavigateP1Right();
		}

		// Analog stick navigation
		HandleP1Analog(delta);

		// Confirm
		if (Input.IsActionJustPressed("p1_confirm"))
		{
			ConfirmP1();
		}
	}

	private void HandleP2Input(float delta)
	{
		if (_p2Ready) return; // P2 can't change selection after ready

		// Left/Right navigation (digital)
		if (Input.IsActionJustPressed("p2_left"))
		{
			NavigateP2Left();
		}
		else if (Input.IsActionJustPressed("p2_right"))
		{
			NavigateP2Right();
		}

		// Analog stick navigation
		HandleP2Analog(delta);

		// Confirm
		if (Input.IsActionJustPressed("p2_confirm"))
		{
			ConfirmP2();
		}
	}

	private void HandleStartInput()
	{
		// Cooldown to prevent multiple scene changes
		if (_startCooldown > 0f)
		{
			_startCooldown -= (float)GetProcessDeltaTime();
			return;
		}

		// Only P1/Controller 1 can start the game, and only when both are ready
		if (_p1Ready && _p2Ready)
		{
			// Use InputMap action (properly mapped in project.godot with just-pressed semantics)
			bool startPressed = Input.IsActionJustPressed("p1_start");
			
			if (startPressed)
			{
				GD.Print("[CHAR_SELECT] CONTROLLER 1 START PRESSED (via p1_start action) - Loading gameplay");
				_startCooldown = START_COOLDOWN_TIME;
				StartGame();
			}
			else
			{
				// Also check direct joypad button for controller without action mapping
				bool joyStartPressed = Input.IsJoyButtonPressed(P1_CONTROLLER_ID, JoyButton.Start);
				if (joyStartPressed)
				{
					GD.Print("[CHAR_SELECT] CONTROLLER 1 START PRESSED (via direct JoyButton.Start) - Loading gameplay");
					_startCooldown = START_COOLDOWN_TIME;
					StartGame();
				}
			}
		}
	}

	private void HandleP1Analog(float delta)
	{
		if (_p1AnalogCooldown > 0f)
		{
			_p1AnalogCooldown -= delta;
			return;
		}

		float analogX = Input.GetJoyAxis(P1_CONTROLLER_ID, JoyAxis.LeftX);

		if (Mathf.Abs(analogX) > ANALOG_DEADZONE)
		{
			if (analogX < -ANALOG_DEADZONE)
			{
				NavigateP1Left();
			}
			else if (analogX > ANALOG_DEADZONE)
			{
				NavigateP1Right();
			}
			_p1AnalogCooldown = ANALOG_COOLDOWN_TIME;
		}
	}

	private void HandleP2Analog(float delta)
	{
		if (_p2AnalogCooldown > 0f)
		{
			_p2AnalogCooldown -= delta;
			return;
		}

		float analogX = Input.GetJoyAxis(P2_CONTROLLER_ID, JoyAxis.LeftX);

		if (Mathf.Abs(analogX) > ANALOG_DEADZONE)
		{
			if (analogX < -ANALOG_DEADZONE)
			{
				NavigateP2Left();
			}
			else if (analogX > ANALOG_DEADZONE)
			{
				NavigateP2Right();
			}
			_p2AnalogCooldown = ANALOG_COOLDOWN_TIME;
		}
	}

	private void NavigateP1Left()
	{
		_p1Selection = GameManager.Character.Gatot;
		UpdateP1Display();
		GD.Print($"P1 selected: {_p1Selection}");
	}

	private void NavigateP1Right()
	{
		_p1Selection = GameManager.Character.Gilgamesh;
		UpdateP1Display();
		GD.Print($"P1 selected: {_p1Selection}");
	}

	private void NavigateP2Left()
	{
		_p2Selection = GameManager.Character.Gatot;
		UpdateP2Display();
		GD.Print($"P2 selected: {_p2Selection}");
	}

	private void NavigateP2Right()
	{
		_p2Selection = GameManager.Character.Gilgamesh;
		UpdateP2Display();
		GD.Print($"P2 selected: {_p2Selection}");
	}

	private void ConfirmP1()
	{
		// Check if P2 has already confirmed the same character
		if (_p2Ready && _p2Selection == _p1Selection)
		{
			GD.Print("[CHAR_SELECT] P1 cannot confirm: character already taken by P2");
			return;
		}

		_p1Ready = true;
		UpdateP1Display();
		GD.Print($"[CHAR_SELECT] P1 READY: {_p1Selection}");
		CheckBothReady();
	}

	private void ConfirmP2()
	{
		// Check if P1 has already confirmed the same character
		if (_p1Ready && _p1Selection == _p2Selection)
		{
			GD.Print("[CHAR_SELECT] P2 cannot confirm: character already taken by P1");
			return;
		}

		_p2Ready = true;
		UpdateP2Display();
		GD.Print($"[CHAR_SELECT] P2 READY: {_p2Selection}");
		CheckBothReady();
	}

	private void CheckBothReady()
	{
		GD.Print($"[CHAR_SELECT] CheckBothReady: P1Ready={_p1Ready}, P2Ready={_p2Ready}");
		if (_p1Ready && _p2Ready)
		{
			GD.Print("[CHAR_SELECT] === BOTH PLAYERS READY ===");
			GD.Print($"[CHAR_SELECT] P1: {_p1Selection}");
			GD.Print($"[CHAR_SELECT] P2: {_p2Selection}");
			// Show PRESS START label
			_startLabel.Visible = true;
		}
	}

	private void StartGame()
	{
		GD.Print("[CHAR_SELECT] TRANSITION REQUESTED");
		GD.Print($"[CHAR_SELECT] P1: {_p1Selection}");
		GD.Print($"[CHAR_SELECT] P2: {_p2Selection}");
		
		// Save selections to GameManager
		_gameManager.SetSelections(_p1Selection, _p2Selection);
		
		// Transition to gameplay scene
		GetTree().ChangeSceneToFile("res://Scenes/main.tscn");
	}

	private void UpdateP1Display()
	{
		// Update portrait
		if (_p1Selection == GameManager.Character.Gatot)
		{
			_p1Portrait.Texture = (Texture2D)GD.Load("res://Character Assets/Gatot/Block/BlockG1.png");
			_p1NameLabel.Text = "GATOT";
		}
		else
		{
			_p1Portrait.Texture = (Texture2D)GD.Load("res://Character Assets/Gilgamesh/Block/frame-1.png");
			_p1NameLabel.Text = "GILGAMESH";
		}

		// Update ready label
		if (_p1Ready)
		{
			_p1ReadyLabel.Text = "READY";
			_p1ReadyLabel.AddThemeColorOverride("font_color", Colors.Green);
		}
		else
		{
			// Show TAKEN if P2 has confirmed this character
			if (_p2Ready && _p2Selection == _p1Selection)
			{
				_p1ReadyLabel.Text = "TAKEN";
				_p1ReadyLabel.AddThemeColorOverride("font_color", Colors.Red);
			}
			else
			{
				_p1ReadyLabel.Text = "PRESS A TO CONFIRM";
				_p1ReadyLabel.AddThemeColorOverride("font_color", Colors.White);
			}
		}

		// Update selection indicator (border/color)
		UpdateSelectionIndicator(_p1Portrait, !_p1Ready);
	}

	private void UpdateP2Display()
	{
		// Update portrait
		if (_p2Selection == GameManager.Character.Gatot)
		{
			_p2Portrait.Texture = (Texture2D)GD.Load("res://Character Assets/Gatot/Block/BlockG1.png");
			_p2NameLabel.Text = "GATOT";
		}
		else
		{
			_p2Portrait.Texture = (Texture2D)GD.Load("res://Character Assets/Gilgamesh/Block/frame-1.png");
			_p2NameLabel.Text = "GILGAMESH";
		}

		// Update ready label
		if (_p2Ready)
		{
			_p2ReadyLabel.Text = "READY";
			_p2ReadyLabel.AddThemeColorOverride("font_color", Colors.Green);
		}
		else
		{
			// Show TAKEN if P1 has confirmed this character
			if (_p1Ready && _p1Selection == _p2Selection)
			{
				_p2ReadyLabel.Text = "TAKEN";
				_p2ReadyLabel.AddThemeColorOverride("font_color", Colors.Red);
			}
			else
			{
				_p2ReadyLabel.Text = "PRESS A TO CONFIRM";
				_p2ReadyLabel.AddThemeColorOverride("font_color", Colors.White);
			}
		}

		// Update selection indicator
		UpdateSelectionIndicator(_p2Portrait, !_p2Ready);
	}

	private void UpdateSelectionIndicator(TextureRect portrait, bool isActive)
	{
		// Visual indicator for active selection
		if (isActive)
		{
			portrait.Modulate = Colors.White;
		}
		else
		{
			portrait.Modulate = new Color(1, 1, 1, 0.5f); // Dimmed when ready
		}
	}

	private void UpdateStartLabelPosition()
	{
		if (_startLabel != null && _startLabel.Visible && IsInsideTree())
		{
			// Position at bottom center
			_startLabel.GlobalPosition = new Vector2(GetViewportRect().Size.X / 2, GetViewportRect().Size.Y - 100);
		}
	}
}