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
	// SFX PLAYERS
	// =========================

	private AudioStreamPlayer2D sfxJump;
	private AudioStreamPlayer2D sfxLand;
	private AudioStreamPlayer2D sfxWalk;
	private AudioStreamPlayer2D sfxPunch;
	private AudioStreamPlayer2D sfxHeavyPunch;
	private AudioStreamPlayer2D sfxBlock;
	private AudioStreamPlayer2D sfxStun;

	// =========================
	// MOVEMENT STATE
	// =========================

	private bool isStunned = false;

	private float jumpDirection = 0.0f;

	// For landing detection
	private bool wasOnFloor = true;

	// For walk sound management
	private bool isWalkPlaying = false;

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

		// Get SFX players
		sfxJump = GetNode<AudioStreamPlayer2D>("SFX_Jump");
		sfxLand = GetNode<AudioStreamPlayer2D>("SFX_Land");
		sfxWalk = GetNode<AudioStreamPlayer2D>("SFX_Walk");
		sfxPunch = GetNode<AudioStreamPlayer2D>("SFX_Punch");
		sfxHeavyPunch = GetNode<AudioStreamPlayer2D>("SFX_HeavyPunch");
		sfxBlock = GetNode<AudioStreamPlayer2D>("SFX_Block");
		sfxStun = GetNode<AudioStreamPlayer2D>("SFX_Stun");

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

		wasOnFloor = IsOnFloor();
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
		// LANDING DETECTION (must be before gravity/movement)
		// =====================================

		bool isOnFloorNow = IsOnFloor();
		if (!wasOnFloor && isOnFloorNow)
		{
			OnLanded();
		}
		wasOnFloor = isOnFloorNow;

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
		// WALK SOUND MANAGEMENT
		// =====================================

		HandleWalkSound();

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

		// Play jump sound
		if (sfxJump != null && !sfxJump.Playing)
		{
			sfxJump.Play();
		}
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

		// Stop walk sound if playing
		if (isWalkPlaying && sfxWalk != null && sfxWalk.Playing)
		{
			sfxWalk.Stop();
			isWalkPlaying = false;
		}

		sprite.Frame = 0;
		sprite.Play("Punch");

		// Play punch sound
		if (sfxPunch != null && !sfxPunch.Playing)
		{
			sfxPunch.Play();
		}
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

		// Stop walk sound if playing
		if (isWalkPlaying && sfxWalk != null && sfxWalk.Playing)
		{
			sfxWalk.Stop();
			isWalkPlaying = false;
		}

		sprite.Frame = 0;

		sprite.Play("Block");

		// Play block sound
		if (sfxBlock != null && !sfxBlock.Playing)
		{
			sfxBlock.Play();
		}

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

			// Stop walk sound if playing
			if (isWalkPlaying && sfxWalk != null && sfxWalk.Playing)
			{
				sfxWalk.Stop();
				isWalkPlaying = false;
			}

			sprite.Play("Hitted");

			// Play stun sound
			if (sfxStun != null && !sfxStun.Playing)
			{
				sfxStun.Play();
			}

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
	// LANDING
	// =========================================================

	private void OnLanded()
	{
		// Play land sound
		if (sfxLand != null && !sfxLand.Playing)
		{
			sfxLand.Play();
		}
	}

	// =========================================================
	// WALK SOUND MANAGEMENT
	// =========================================================

	private void HandleWalkSound()
	{
		bool shouldWalk = IsOnFloor() && Mathf.Abs(Velocity.X) > 0.1f && !isPunching && !isBlocking;

		if (shouldWalk && !isWalkPlaying)
		{
			if (sfxWalk != null && !sfxWalk.Playing)
			{
				sfxWalk.Play();
			}
			isWalkPlaying = true;
		}
		else if (!shouldWalk && isWalkPlaying)
		{
			if (sfxWalk != null && sfxWalk.Playing)
			{
				sfxWalk.Stop();
			}
			isWalkPlaying = false;
		}
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
