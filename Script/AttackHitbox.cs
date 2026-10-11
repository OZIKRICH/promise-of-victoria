using Godot;

public partial class AttackHitbox : Area2D
{
	[Export]
	public int Damage = 10;

	private bool alreadyHit = false;

	private PlayerMovement attacker;

	public override void _Ready()
	{
		AreaEntered += OnAreaEntered;

		SetDeferred(
			"monitoring",
			false
		);

		attacker = GetParent<PlayerMovement>();

		GD.Print(
			"AttackHitbox siap: ",
			GetParent().Name
		);
	}

	public void StartAttack()
	{
		alreadyHit = false;

		SetDeferred(
			"monitoring",
			true
		);

		GD.Print(
			GetParent().Name,
			" AttackHitbox AKTIF"
		);
	}

	public void EndAttack()
	{
		SetDeferred(
			"monitoring",
			false
		);

		GD.Print(
			GetParent().Name,
			" AttackHitbox MATI"
		);
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

		// =====================================================
		// CEK APAKAH HURTBOX MILIK DIRI SENDIRI
		// =====================================================

		Node targetPlayer = area.GetParent();

		if (targetPlayer == GetParent())
		{
			GD.Print(
				GetParent().Name,
				" menyerang dirinya sendiri → DIABAIKAN"
			);

			return;
		}

		// =====================================================
		// CARI PLAYER HEALTH TARGET
		// =====================================================

		PlayerHealth health =
			targetPlayer.GetNodeOrNull<PlayerHealth>(
				"PlayerHealth"
			);

		if (health == null)
		{
			GD.Print(
				"PlayerHealth tidak ditemukan dari: ",
				area.Name
			);

			return;
		}

		// =====================================================
		// HIT
		// =====================================================

		alreadyHit = true;

		GD.Print(
			GetParent().Name,
			" HIT → ",
			targetPlayer.Name,
			" | Damage: ",
			Damage
		);

		health.ReceiveAttack(
			Damage,
			attacker
		);
	}
}
