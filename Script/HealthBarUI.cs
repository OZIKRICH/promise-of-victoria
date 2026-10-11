using Godot;

public partial class HealthBarUI : CanvasLayer
{
	private ProgressBar p1HealthBar;
	private ProgressBar p2HealthBar;
	private Label p1Label;
	private Label p2Label;

	private PlayerHealth p1Health;
	private PlayerHealth p2Health;

	public override void _Ready()
	{
		// Get UI nodes (direct children of UIRoot)
		p1HealthBar = GetNode<ProgressBar>("UIRoot/P1HealthBar");
		p2HealthBar = GetNode<ProgressBar>("UIRoot/P2HealthBar");
		p1Label = GetNode<Label>("UIRoot/P1Label");
		p2Label = GetNode<Label>("UIRoot/P2Label");

		// Configure progress bars
		SetupHealthBar(p1HealthBar);
		SetupHealthBar(p2HealthBar);

		// Find PlayerHealth nodes in the scene (deferred to run after Main spawns characters)
		CallDeferred(nameof(FindPlayerHealthNodes));

		if (p1Health != null)
		{
			p1Health.HealthChanged += OnP1HealthChanged;
			// Set initial value
			UpdateHealthBar(p1HealthBar, p1Health.CurrentHealth, p1Health.MaxHealth);
		}

		if (p2Health != null)
		{
			p2Health.HealthChanged += OnP2HealthChanged;
			// Set initial value
			UpdateHealthBar(p2HealthBar, p2Health.CurrentHealth, p2Health.MaxHealth);
		}

		GD.Print("HealthBarUI initialized");
	}

	private void SetupHealthBar(ProgressBar bar)
	{
		bar.MinValue = 0;
		bar.MaxValue = 100;
		bar.Value = 100;
	}

	private void FindPlayerHealthNodes()
	{
		// Find P1 and P2 (spawned by Main.cs with names "P1" and "P2")
		var p1Node = GetTree().Root.GetNodeOrNull<Node2D>("Main/P1");
		var p2Node = GetTree().Root.GetNodeOrNull<Node2D>("Main/P2");

		if (p1Node != null)
		{
			p1Health = p1Node.GetNodeOrNull<PlayerHealth>("PlayerHealth");
			GD.Print($"Found P1 Health: {p1Health != null}");
			if (p1Health != null)
			{
				p1Health.HealthChanged += OnP1HealthChanged;
				UpdateHealthBar(p1HealthBar, p1Health.CurrentHealth, p1Health.MaxHealth);
			}
		}

		if (p2Node != null)
		{
			p2Health = p2Node.GetNodeOrNull<PlayerHealth>("PlayerHealth");
			GD.Print($"Found P2 Health: {p2Health != null}");
			if (p2Health != null)
			{
				p2Health.HealthChanged += OnP2HealthChanged;
				UpdateHealthBar(p2HealthBar, p2Health.CurrentHealth, p2Health.MaxHealth);
			}
		}
	}

	private void OnP1HealthChanged(int current, int max)
	{
		GD.Print($"P1 Health Changed: {current}/{max}");
		UpdateHealthBar(p1HealthBar, current, max);
	}

	private void OnP2HealthChanged(int current, int max)
	{
		GD.Print($"P2 Health Changed: {current}/{max}");
		UpdateHealthBar(p2HealthBar, current, max);
	}

	private void UpdateHealthBar(ProgressBar bar, int current, int max)
	{
		if (bar == null) return;

		float percent = max > 0 ? (float)current / max * 100f : 0f;
		bar.Value = Mathf.Clamp(percent, 0, 100);
	}
}