using Godot;

public partial class PlayerHealth : Node
{
	[Export]
	public int MaxHealth = 100;

	[Export]
	public float StunDuration = 0.4f;

	[Export]
	public float PerfectBlockStun = 0.35f;

	[Export]
	public float GuardBreakStun = 1.2f;

	[Export]
	public float NormalBlockDamageMultiplier = 0.5f;

	private int currentHealth;

	private AnimatedSprite2D sprite;
	private PlayerMovement movement;

	private bool isStunned = false;

	// Signal emitted when health changes
	[Signal]
	public delegate void HealthChangedEventHandler(int currentHealth, int maxHealth);

	// Public read-only access to current health
	public int CurrentHealth => currentHealth;

	public override void _Ready()
	{
		currentHealth = MaxHealth;

		sprite = GetParent().GetNode<AnimatedSprite2D>(
			"AnimatedSprite2D"
		);

		movement = GetParent().GetNode<PlayerMovement>(".");

		sprite.SpriteFrames.SetAnimationLoop(
			"Hitted",
			false
		);

		GD.Print(
			GetParent().Name,
			" PlayerHealth siap. HP = ",
			currentHealth
		);

		// Emit initial health
		EmitSignal(SignalName.HealthChanged, currentHealth, MaxHealth);
	}

	// =========================================================
	// ATTACK RECEIVED
	// =========================================================

	public async void ReceiveAttack(
		int damage,
		PlayerMovement attacker
	)
	{
		if (currentHealth <= 0)
		{
			return;
		}

		// =====================================================
		// PERFECT BLOCK
		// =====================================================

		if (movement.IsPerfectBlocking())
		{
			GD.Print(
				GetParent().Name,
				" PERFECT BLOCK!"
			);

			// Tidak ada damage
			GD.Print(
				GetParent().Name,
				" tidak menerima damage"
			);

			// Attacker yang kena stun
			if (attacker != null)
			{
				attacker.SetStunned(true);

				PlayAttackerHitted(attacker);

				await ToSignal(
					GetTree().CreateTimer(
						PerfectBlockStun
					),
					SceneTreeTimer.SignalName.Timeout
				);

				if (IsInstanceValid(attacker))
				{
					attacker.SetStunned(false);
				}
			}

			return;
		}

		// =====================================================
		// GUARD BREAK
		// =====================================================

		if (movement.IsGuardBroken())
		{
			int heavyDamage = damage * 2;

			GD.Print(
				GetParent().Name,
				" GUARD BREAK / HEAVY HIT!"
			);

			// Play heavy punch sound on attacker
			if (attacker != null)
			{
				var sfxHeavyPunch = attacker.GetNodeOrNull<AudioStreamPlayer2D>("SFX_HeavyPunch");
				if (sfxHeavyPunch != null && !sfxHeavyPunch.Playing)
				{
					sfxHeavyPunch.Play();
				}
			}

			TakeDamage(
				heavyDamage,
				GuardBreakStun
			);

			return;
		}

		// =====================================================
		// NORMAL BLOCK
		// =====================================================

		if (movement.IsBlocking())
		{
			int blockedDamage = Mathf.Max(
				1,
				Mathf.RoundToInt(
					damage * NormalBlockDamageMultiplier
				)
			);

			GD.Print(
				GetParent().Name,
				" NORMAL BLOCK → Damage ",
				blockedDamage
			);

			TakeDamage(
				blockedDamage,
				StunDuration
			);

			return;
		}

		// =====================================================
		// NORMAL HIT
		// =====================================================

		TakeDamage(
			damage,
			StunDuration
		);
	}

	// =========================================================
	// DAMAGE
	// =========================================================

	private async void TakeDamage(
		int damage,
		float stunDuration
	)
	{
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

		// Emit health changed signal for UI
		EmitSignal(SignalName.HealthChanged, currentHealth, MaxHealth);

		isStunned = true;

		movement.SetStunned(true);

		sprite.Play("Hitted");

		await ToSignal(
			GetTree().CreateTimer(
				stunDuration
			),
			SceneTreeTimer.SignalName.Timeout
		);

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

	// =========================================================
	// ATTACKER HITTED BY PERFECT BLOCK
	// =========================================================

	private void PlayAttackerHitted(
		PlayerMovement attacker
	)
	{
		AnimatedSprite2D attackerSprite =
			attacker.GetNode<AnimatedSprite2D>(
				"AnimatedSprite2D"
			);

		attackerSprite.Play("Hitted");
	}
}
