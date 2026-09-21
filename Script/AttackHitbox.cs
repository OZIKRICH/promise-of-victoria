using Godot;

public partial class AttackHitbox : Area2D
{
	[Export]
	public int Damage = 10;

	private bool alreadyHit = false;

	public override void _Ready()
	{
		AreaEntered += OnAreaEntered;

		// Hitbox mati saat game dimulai
		SetDeferred("monitoring", false);

		GD.Print(
			"AttackHitbox siap: ",
			GetParent().Name
		);
	}

	public void StartAttack()
	{
		alreadyHit = false;

		// Aktifkan setelah physics frame aman
		SetDeferred("monitoring", true);

		GD.Print("AttackHitbox AKTIF");
	}

	public void EndAttack()
	{
		// Matikan setelah physics frame aman
		SetDeferred("monitoring", false);

		GD.Print("AttackHitbox MATI");
	}

	private void OnAreaEntered(Area2D area)
	{
		if (!Monitoring)
		{
			return;
		}

		if (alreadyHit)
		{
			return;
		}

		GD.Print(
			"AttackHitbox mendeteksi: ",
			area.Name
		);

		PlayerHealth health = area.GetParent()
			.GetNodeOrNull<PlayerHealth>("PlayerHealth");

		if (health == null)
		{
			GD.Print(
				"PlayerHealth tidak ditemukan dari: ",
				area.Name
			);

			return;
		}

		alreadyHit = true;

		GD.Print(
			"HIT! Damage: ",
			Damage
		);

		health.TakeDamage(Damage);
	}
}
