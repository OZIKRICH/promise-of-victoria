using Godot;

public partial class CombatCollision : Node
{
	// ==========================================
	// UKURAN COLLISION (berbasis ukuran sprite ter-render)
	// Sprite: ~330x518 source, scale 0.469x0.465 → ~155x241 rendered
	// ==========================================

	// Body player - collision fisik dengan lingkungan
	[Export]
	private Vector2 bodySize = new Vector2(100, 200);

	// Area yang bisa terkena serangan (hurtbox)
	[Export]
	private Vector2 hurtboxSize = new Vector2(90, 190);

	// Area pukulan (attack hitbox) - diatur posisi oleh PlayerMovement
	[Export]
	private Vector2 attackHitboxSize = new Vector2(60, 80);

	// ==========================================
	// POSISI COLLISION (relatif ke CharacterBody2D origin di kaki)
	// ==========================================

	// Body di tengah karakter, dari kaki (y=0) ke atas
	[Export]
	private Vector2 bodyOffset = new Vector2(0, -100);

	// Hurtbox sedikit lebih kecil, juga dari kaki ke atas
	[Export]
	private Vector2 hurtboxOffset = new Vector2(0, -95);

	// Attack hitbox offset - digunakan oleh PlayerMovement untuk positioning
	[Export]
	private Vector2 attackHitboxOffset = new Vector2(45, -40);

	public override void _Ready()
	{
		Node player = GetParent();

		// ==========================================
		// BODY COLLISION
		// ==========================================

		CollisionShape2D body =
			player.GetNode<CollisionShape2D>(
				"CollisionShape2D"
			);

		body.Shape = CreateRectangle(bodySize);
		body.Position = bodyOffset;
		body.Scale = Vector2.One;
		body.Visible = true;
		body.Disabled = false;

		// DEBUG: Verify shape reference
		GD.Print("Body shape reference after assign: ", body.Shape.GetInstanceId(), " size: ", (body.Shape as RectangleShape2D).Size);
		GD.Print("Body node path: ", body.GetPath(), " owner: ", body.GetParent().Name);
		GD.Print("Body disabled: ", body.Disabled, " visible: ", body.Visible);

		// Force physics update by toggling disabled
		body.Disabled = true;
		body.Disabled = false;

		// ==========================================
		// HURTBOX
		// ==========================================

		CollisionShape2D hurtbox =
			player.GetNode<CollisionShape2D>(
				"Hurtbox/CollisionShape2D"
			);

		hurtbox.Shape = CreateRectangle(hurtboxSize);
		hurtbox.Position = hurtboxOffset;

		// DEBUG: Verify hurtbox shape reference
		GD.Print("Hurtbox shape reference after assign: ", hurtbox.Shape.GetInstanceId(), " size: ", (hurtbox.Shape as RectangleShape2D).Size);

		// Hurtbox Area2D harus di posisi (0,0) relativ ke player
		Area2D hurtboxArea = player.GetNode<Area2D>("Hurtbox");
		hurtboxArea.Position = Vector2.Zero;

		// ==========================================
		// ATTACK HITBOX
		// ==========================================

		CollisionShape2D attack =
			player.GetNode<CollisionShape2D>(
				"AttackHitbox/CollisionShape2D"
			);

		attack.Shape = CreateRectangle(attackHitboxSize);
		attack.Position = Vector2.Zero; // Centered di AttackHitbox Area2D

		// DEBUG: Verify attack shape reference
		GD.Print("Attack shape reference after assign: ", attack.Shape.GetInstanceId(), " size: ", (attack.Shape as RectangleShape2D).Size);

		// AttackHitbox Area2D posisi diatur oleh PlayerMovement.UpdateAttackHitboxPosition()
		Area2D attackHitboxArea = player.GetNode<Area2D>("AttackHitbox");
		attackHitboxArea.Position = Vector2.Zero;
		attackHitboxArea.Monitoring = false; // Start disabled

		GD.Print(
			player.Name,
			" collision generated: Body=",
			bodySize,
			" Hurtbox=",
			hurtboxSize,
			" Attack=",
			attackHitboxSize
		);
	}

	private RectangleShape2D CreateRectangle(
		Vector2 size
	)
	{
		RectangleShape2D rectangle =
			new RectangleShape2D();

		rectangle.Size = size;

		return rectangle;
	}
}
