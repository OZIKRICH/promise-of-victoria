using Godot;

public partial class PlayerMovement : CharacterBody2D
{
	[Export] private float speed = 200.0f;
	[Export] private float gravity = 980.0f;
	[Export] private float jumpForce = 500.0f;

	private AnimatedSprite2D sprite;
	private PlayerInput playerInput;
	private AttackHitbox attackHitbox;

	private bool isStunned = false;
	private bool isPunching = false;
	private bool punchWasPressed = false;

	private float jumpDirection = 0.0f;

	public override void _Ready()
	{
		sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		playerInput = GetNode<PlayerInput>("PlayerInput");
		attackHitbox = GetNode<AttackHitbox>("AttackHitbox");

		sprite.SpriteFrames.SetAnimationLoop("Punch", false);
		sprite.SpriteFrames.SetAnimationLoop("Hitted", false);

		sprite.AnimationFinished += OnAnimationFinished;

		sprite.Play("Idle");
	}

	public override void _PhysicsProcess(double delta)
	{
		if (isStunned)
		{
			Velocity = Vector2.Zero;
			MoveAndSlide();
			return;
		}

		HandleGravity(delta);

		HandlePunch();

		if (!isPunching)
		{
			HandleMovement();
			HandleJump();
		}

		HandleAnimation();

		MoveAndSlide();
	}

	private void HandleGravity(double delta)
	{
		if (!IsOnFloor())
		{
			Velocity += Vector2.Down * gravity * (float)delta;
		}
	}

	private void HandleMovement()
	{
		float direction = playerInput.GetMoveDirection();

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
		}
	}

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

		float direction = playerInput.GetMoveDirection();

		jumpDirection = direction;

		Velocity = new Vector2(
			jumpDirection * speed,
			-jumpForce
		);
	}

	private void HandlePunch()
	{
		bool punchPressed = playerInput.IsPunchPressed();

		// Hanya mendeteksi PRESS baru
		if (punchPressed && !punchWasPressed)
		{
			if (!isPunching && IsOnFloor())
			{
				isPunching = true;

				Velocity = new Vector2(
					0,
					Velocity.Y
				);

				// Aktifkan hitbox hanya saat punch
				attackHitbox.StartAttack();

				sprite.Frame = 0;
				sprite.Play("Punch");
			}
		}

		punchWasPressed = punchPressed;
	}

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

	private void OnAnimationFinished()
	{
		if (sprite.Animation != "Punch")
		{
			return;
		}

		isPunching = false;

		// Matikan hitbox setelah punch selesai
		attackHitbox.EndAttack();

		Velocity = new Vector2(
			0,
			Velocity.Y
		);

		sprite.Play("Idle");
	}

	public void SetStunned(bool value)
	{
		isStunned = value;

		if (value)
		{
			Velocity = Vector2.Zero;

			// Pastikan hitbox mati ketika terkena stun
			attackHitbox.EndAttack();
		}
	}
}
