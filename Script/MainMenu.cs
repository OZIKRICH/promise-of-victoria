using Godot;
using System;

public partial class MainMenu : Control
{
	private enum MenuOption
	{
		Play = 0,
		Options = 1,
		Exit = 2
	}

	private MenuOption _currentSelection = MenuOption.Play;
	private Label _titleLabel;
	private Button _playButton;
	private Button _optionsButton;
	private Button _exitButton;
	private Label _hintLabel;
	private Button[] _menuButtons;

	private const int CONTROLLER_ID = 0; // Controller 1
	private const float ANALOG_DEADZONE = 0.3f;
	private float _analogCooldown = 0f;

	public override void _Ready()
	{
		// Get UI elements using exact runtime names (with # prefix from scene)
		_titleLabel = FindChild("TitleLabel") as Label;
		_playButton = FindChild("MenuContainer#PlayLabel") as Button;
		_optionsButton = FindChild("MenuContainer#OptionsLabel") as Button;
		_exitButton = FindChild("MenuContainer#ExitLabel") as Button;
		_hintLabel = FindChild("HintLabel") as Label;

		_menuButtons = [_playButton, _optionsButton, _exitButton];

		// Connect button pressed signals (mouse click)
		_playButton.Pressed += OnPlaySelected;
		_optionsButton.Pressed += OnOptionsSelected;
		_exitButton.Pressed += OnExitSelected;

		// Connect focus entered signals for mouse hover -> keyboard/controller sync
		_playButton.FocusEntered += () => OnButtonFocusEntered(MenuOption.Play);
		_optionsButton.FocusEntered += () => OnButtonFocusEntered(MenuOption.Options);
		_exitButton.FocusEntered += () => OnButtonFocusEntered(MenuOption.Exit);

		// Ensure mouse cursor is visible
		Input.MouseMode = Input.MouseModeEnum.Visible;

		// Set initial focus to Play button for keyboard/controller navigation
		_playButton.GrabFocus();

		UpdateSelectionDisplay();
	}

	public override void _Process(double delta)
	{
		HandleInput();
		HandleAnalogInput((float)delta);
	}

	private void HandleInput()
	{
		// Navigation - using IsActionJustPressed for debounce
		if (Input.IsActionJustPressed("menu_up"))
		{
			NavigateUp();
		}
		else if (Input.IsActionJustPressed("menu_down"))
		{
			NavigateDown();
		}

		// Confirm - works on focused button (keyboard Enter/Space or controller A/Cross)
		if (Input.IsActionJustPressed("menu_confirm"))
		{
			ConfirmFocusedButton();
		}

		// Back (Escape) - in main menu, this exits the game
		if (Input.IsActionJustPressed("menu_back"))
		{
			OnBackPressed();
		}
	}

	private void HandleAnalogInput(float delta)
	{
		// Cooldown to prevent rapid analog navigation
		if (_analogCooldown > 0f)
		{
			_analogCooldown -= delta;
			return;
		}

		// Only check Controller 1 (ControllerId = 0)
		float analogY = Input.GetJoyAxis(CONTROLLER_ID, JoyAxis.LeftY);

		if (Mathf.Abs(analogY) > ANALOG_DEADZONE)
		{
			if (analogY < -ANALOG_DEADZONE)
			{
				NavigateUp();
			}
			else if (analogY > ANALOG_DEADZONE)
			{
				NavigateDown();
			}
			_analogCooldown = 0.2f; // 200ms debounce
		}
	}

	private void NavigateUp()
	{
		_currentSelection = (MenuOption)(((int)_currentSelection - 1 + 3) % 3);
		UpdateSelectionDisplay();
		_menuButtons[(int)_currentSelection].GrabFocus();
	}

	private void NavigateDown()
	{
		_currentSelection = (MenuOption)(((int)_currentSelection + 1) % 3);
		UpdateSelectionDisplay();
		_menuButtons[(int)_currentSelection].GrabFocus();
	}

	private void OnButtonFocusEntered(MenuOption option)
	{
		// Sync selection when mouse hovers/focuses a button
		_currentSelection = option;
		UpdateSelectionDisplay();
	}

	private void UpdateSelectionDisplay()
	{
		// Update button texts to show selection indicator
		_playButton.Text = _currentSelection == MenuOption.Play ? "> PLAY" : "  PLAY";
		_optionsButton.Text = _currentSelection == MenuOption.Options ? "> OPTIONS" : "  OPTIONS";
		_exitButton.Text = _currentSelection == MenuOption.Exit ? "> EXIT" : "  EXIT";
	}

	private void ConfirmFocusedButton()
	{
		// Trigger the currently focused button's pressed action
		var focusedButton = _menuButtons[(int)_currentSelection];
		if (focusedButton != null)
		{
			focusedButton.EmitSignal(Button.SignalName.Pressed);
		}
	}

	private void OnPlaySelected()
	{
		GD.Print("PLAY SELECTED - Loading Character Select");
		GetTree().ChangeSceneToFile("res://Scenes/CharacterSelect.tscn");
	}

	private void OnOptionsSelected()
	{
		GD.Print("OPTIONS SELECTED");
		// TODO: Open options menu when implemented
	}

	private void OnExitSelected()
	{
		GD.Print("EXIT SELECTED - Quitting game");
		GetTree().Quit();
	}

	private void OnBackPressed()
	{
		// In main menu, Escape acts as Exit
		OnExitSelected();
	}
}