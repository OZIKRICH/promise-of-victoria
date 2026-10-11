using Godot;

public partial class PlayerMovement : CharacterBody2D
{
	[Export] private float speed = 200.0f;
	[Export] private float gravity = 980.0f;
	[Export] private float jumpForce = 500.0f;

	// =========================
	// BLOCK
	// =========================

	[Export] private float perfectBlockWindow = 0.15f;
	[Export] private float guardBreakTime = 1.0f;

	// =========================
	// ATTACK HITBOX
	// =========================

	[Export]
	private Vector2 attackHitboxOffset = new Vector2(45, -40);

	// =========================
	// COMPONENTS
	// =========================

	private AnimatedSprite2D sprite;
	private PlayerInput playerInput;
	private AttackHitbox attackHitbox;

	// =========================
	// MOVEMENT STATE
	// =========================

	private bool isStunned = false;

	private float jumpDirection = 0.0f;

	// =========================
	// PUNCH STATE
	// =========================

	private bool isPunching = false;
	private bool punchWasPressed = false;

	// =========================
	// BLOCK STATE
	// =========================

	private bool isBlocking = false;
	private bool blockWasPressed = false;

	private double blockStartTime = 0.0;

	private bool guardBroken = false;

	// =========================================================
	// READY
	// =========================================================

	public override void _Ready()
	{
		sprite = GetNode<AnimatedSprite2D>(
			"AnimatedSprite2D"
		);

		playerInput = GetNode<PlayerInput>(
			"PlayerInput"
		);

		attackHitbox = GetNode<AttackHitbox>(
			"AttackHitbox"
		);

		sprite.SpriteFrames.SetAnimationLoop(
			"Punch",
			false
		);

		sprite.SpriteFrames.SetAnimationLoop(
			"Hitted",
			false
		);

		sprite.SpriteFrames.SetAnimationLoop(
			"Block",
			false
		);

		sprite.AnimationFinished += OnAnimationFinished;

		UpdateAttackHitboxPosition();

		sprite.Play("Idle");
	}

	// =========================================================
	// PHYSICS
	// =========================================================

	public override void _PhysicsProcess(double delta)
	{
		// =====================================
		// STUN
		// =====================================

		if (isStunned)
		{
			Velocity = Vector2.Zero;

			MoveAndSlide();

			return;
		}

		// =====================================
		// GRAVITY
		// =====================================

		HandleGravity(delta);

		// =====================================
		// BLOCK
		// =====================================

		HandleBlock();

		// =====================================
		// PUNCH
		// =====================================

		HandlePunch();

		// =====================================
		// MOVEMENT
		// =====================================

		if (!isPunching && !isBlocking)
		{
			HandleMovement();
			HandleJump();
		}

		// =====================================
		// ANIMATION
		// =====================================

		if (!isBlocking)
		{
			HandleAnimation();
		}

		MoveAndSlide();
	}

	// =========================================================
	// GRAVITY
	// =========================================================

	private void HandleGravity(double delta)
	{
		if (!IsOnFloor())
		{
			Velocity += Vector2.Down *
				gravity *
				(float)delta;
		}
	}

	// =========================================================
	// MOVEMENT
	// =========================================================

	private void HandleMovement()
	{
		float direction =
			playerInput.GetMoveDirection();

		if (!IsOnFloor())
		{
			Velocity = new Vector2(
				jumpDirection * speed,
				Velocity.Y
			);

			return;
		}

		Velocity = new Vector2(
			direction * speed,
			Velocity.Y
		);

		if (direction != 0)
		{
			sprite.FlipH = direction < 0;

			UpdateAttackHitboxPosition();
		}
	}

	// =========================================================
	// JUMP
	// =========================================================

	private void HandleJump()
	{
		if (!playerInput.IsJumpPressed())
		{
			return;
		}

		if (!IsOnFloor())
		{
			return;
		}

		float direction =
			playerInput.GetMoveDirection();

		jumpDirection = direction;

		Velocity = new Vector2(
			jumpDirection * speed,
			-jumpForce
		);
	}

	// =========================================================
	// PUNCH
	// =========================================================

	private void HandlePunch()
	{
		bool punchPressed =
			playerInput.IsPunchPressed();

		if (
			punchPressed &&
			!punchWasPressed
		)
		{
			if (
				!isPunching &&
				!isBlocking &&
				IsOnFloor()
			)
			{
				StartPunch();
			}
		}

		punchWasPressed = punchPressed;
	}

	// =========================================================
	// START PUNCH
	// =========================================================

	private void StartPunch()
	{
		isPunching = true;

		Velocity = new Vector2(
			0,
			Velocity.Y
		);

		attackHitbox.StartAttack();

		sprite.Frame = 0;
		sprite.Play("Punch");
	}

	// =========================================================
	// BLOCK
	// =========================================================

	private void HandleBlock()
	{
		bool blockPressed =
			playerInput.IsBlockPressed();

		// =====================================
		// START BLOCK
		// =====================================

		if (
			blockPressed &&
			!blockWasPressed
		)
		{
			if (
				!isPunching &&
				IsOnFloor()
			)
			{
				StartBlock();
			}
		}

		// =====================================
		// BLOCK ACTIVE
		// =====================================

		if (isBlocking)
		{
			Velocity = new Vector2(
				0,
				Velocity.Y
			);

			// Frame 0 → Frame 1
			// lalu freeze.
			if (sprite.Frame == 0)
			{
				sprite.Frame = 1;
				sprite.Pause();
			}

			// =================================
			// GUARD BREAK
			// =================================

			if (
				CalculateBlockTime()
				> guardBreakTime
			)
			{
				guardBroken = true;
			}
		}

		// =====================================
		// RELEASE BLOCK
		// =====================================

		if (
			!blockPressed &&
			blockWasPressed
		)
		{
			if (isBlocking)
			{
				CancelBlock();
			}
		}

		blockWasPressed = blockPressed;
	}

	// =========================================================
	// START BLOCK
	// =========================================================

	private void StartBlock()
	{
		isBlocking = true;

		guardBroken = false;

		blockStartTime =
			Time.GetTicksMsec() / 1000.0;

		Velocity = new Vector2(
			0,
			Velocity.Y
		);

		attackHitbox.EndAttack();

		sprite.Frame = 0;

		sprite.Play("Block");

		GD.Print(
			Name,
			" → BLOCK START"
		);
	}

	// =========================================================
	// CANCEL BLOCK
	// =========================================================

	private void CancelBlock()
	{
		isBlocking = false;

		guardBroken = false;

		blockStartTime = 0.0;

		sprite.Play("Hitted");

		GD.Print(
			Name,
			" → BLOCK CANCEL"
		);
	}

	// =========================================================
	// BLOCK TIME
	// =========================================================

	private float CalculateBlockTime()
	{
		return (float)(
			Time.GetTicksMsec() / 1000.0
			- blockStartTime
		);
	}

	// =========================================================
	// PERFECT BLOCK
	// =========================================================

	public bool IsPerfectBlocking()
	{
		if (!isBlocking)
		{
			return false;
		}

		if (guardBroken)
		{
			return false;
		}

		return CalculateBlockTime()
			<= perfectBlockWindow;
	}

	// =========================================================
	// NORMAL BLOCK
	// =========================================================

	public bool IsBlocking()
	{
		return isBlocking &&
			!guardBroken;
	}

	// =========================================================
	// GUARD BREAK
	// =========================================================

	public bool IsGuardBroken()
	{
		return isBlocking &&
			guardBroken;
	}

	// =========================================================
	// GET BLOCK TIME
	// =========================================================

	public float GetBlockTime()
	{
		return CalculateBlockTime();
	}

	// =========================================================
	// STUN
	// =========================================================

	public void SetStunned(bool value)
	{
		isStunned = value;

		if (value)
		{
			// =================================
			// RESET SEMUA STATE
			// =================================

			isPunching = false;

			isBlocking = false;

			guardBroken = false;

			blockStartTime = 0.0;

			// Supaya setelah stun selesai,
			// input bisa digunakan lagi.
			punchWasPressed = false;
			blockWasPressed = false;

			Velocity = Vector2.Zero;

			attackHitbox.EndAttack();

			sprite.Play("Hitted");

			GD.Print(
				Name,
				" → STUN"
			);
		}
		else
		{
			// =================================
			// STUN SELESAI
			// =================================

			isStunned = false;

			isPunching = false;
			isBlocking = false;
			guardBroken = false;

			blockStartTime = 0.0;

			punchWasPressed = false;
			blockWasPressed = false;

			Velocity = Vector2.Zero;

			attackHitbox.EndAttack();

			sprite.Play("Idle");

			GD.Print(
				Name,
				" → STUN SELESAI"
			);
		}
	}

	// =========================================================
	// ATTACK HITBOX POSITION
	// =========================================================

	private void UpdateAttackHitboxPosition()
	{
		float direction =
			sprite.FlipH
				? -1.0f
				: 1.0f;

		attackHitbox.Position =
			new Vector2(
				direction *
				attackHitboxOffset.X,
				attackHitboxOffset.Y
			);
	}

	// =========================================================
	// NORMAL ANIMATION
	// =========================================================

	private void HandleAnimation()
	{
		if (isPunching)
		{
			return;
		}

		if (!IsOnFloor())
		{
			sprite.Play("Jump");

			return;
		}

		if (Mathf.Abs(Velocity.X) > 0.1f)
		{
			sprite.Play("Walk");

			return;
		}

		sprite.Play("Idle");
	}

	// =========================================================
	// ANIMATION FINISHED
	// =========================================================

	private void OnAnimationFinished()
	{
		// =====================================
		// PUNCH
		// =====================================

		if (sprite.Animation == "Punch")
		{
			isPunching = false;

			attackHitbox.EndAttack();

			Velocity = new Vector2(
				0,
				Velocity.Y
			);

			sprite.Play("Idle");

			return;
		}

		// =====================================
		// BLOCK
		// =====================================

		if (sprite.Animation == "Block")
		{
			if (isBlocking)
			{
				sprite.Frame = 1;
				sprite.Pause();

				return;
			}

			sprite.Play("Idle");

			return;
		}

		// =====================================
		// HITTED
		// =====================================

		if (sprite.Animation == "Hitted")
		{
			if (!isStunned)
			{
				sprite.Play("Idle");
			}
		}
	}
}
