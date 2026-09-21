using Godot;

public partial class PlayerHealth : Node
{
	[Export]
	public int MaxHealth = 100;

	[Export]
	public float StunDuration = 0.35f;

	private int currentHealth;

	private AnimatedSprite2D sprite;
	private PlayerMovement movement;

	private bool isStunned = false;

	public override void _Ready()
	{
		currentHealth = MaxHealth;

		sprite = GetParent().GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		movement = GetParent().GetNode<PlayerMovement>(".");

		sprite.SpriteFrames.SetAnimationLoop("Hitted", false);

		GD.Print(
			GetParent().Name,
			" PlayerHealth siap. HP = ",
			currentHealth
		);
	}

	public async void TakeDamage(int damage)
	{
		if (currentHealth <= 0)
		{
			return;
		}

		currentHealth -= damage;

		GD.Print(
			"TAKE DAMAGE → ",
			GetParent().Name,
			" menerima ",
			damage,
			" damage"
		);

		GD.Print(
			GetParent().Name,
			" HP sekarang = ",
			currentHealth
		);

		// Putuskan stun lama kalau sedang stun
		isStunned = true;

		movement.SetStunned(true);

		// Mainkan animasi hit
		sprite.Play("Hitted");

		GD.Print("Hitted + Stun dijalankan!");

		// Tunggu durasi stun
		await ToSignal(
			GetTree().CreateTimer(StunDuration),
			SceneTreeTimer.SignalName.Timeout
		);

		// Kalau HP masih ada, pulihkan player
		if (currentHealth > 0)
		{
			isStunned = false;

			movement.SetStunned(false);

			sprite.Play("Idle");

			GD.Print(
				GetParent().Name,
				" selesai stun"
			);
		}
	}
}
