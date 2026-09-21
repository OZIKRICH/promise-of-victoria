using Godot;

public partial class PlayerInput : Node
{
	[Export]
	public int ControllerId = 0;

	public float GetMoveDirection()
	{
		float direction = 0.0f;

		// D-Pad kiri
		if (Input.IsJoyButtonPressed(
			ControllerId,
			JoyButton.DpadLeft))
		{
			direction -= 1.0f;
		}

		// D-Pad kanan
		if (Input.IsJoyButtonPressed(
			ControllerId,
			JoyButton.DpadRight))
		{
			direction += 1.0f;
		}

		// Analog kiri
		float analog = Input.GetJoyAxis(
			ControllerId,
			JoyAxis.LeftX
		);

		if (Mathf.Abs(analog) > 0.2f)
		{
			direction = analog;
		}

		return direction;
	}

	public bool IsPunchPressed()
	{
		// B / Circle
		return Input.IsJoyButtonPressed(
			ControllerId,
			JoyButton.B
		);
	}

	public bool IsJumpPressed()
	{
		// A / Cross
		return Input.IsJoyButtonPressed(
			ControllerId,
			JoyButton.A
		);
	}
}
