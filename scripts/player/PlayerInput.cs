using Godot;
using System;

public partial class PlayerInput : Node {
	private Vector2 _inputDirection = Vector2.Zero;
	private Vector2 _mouseMovement = Vector2.Zero;
	private bool _JumpPressed = false;
	private bool _CrouchPressed = false;
	private bool _SprintPressed = false;
	private bool _InteractPressed = false;

	public Vector2 InputDirection => _inputDirection;
	public Vector2 MouseMovement => _mouseMovement;
	public bool JumpPressed => _JumpPressed;
	public bool CrouchPressed => _CrouchPressed;
	public bool SprintPressed => _SprintPressed;
	public bool InteractPressed => _InteractPressed;

	public override void _UnhandledInput(InputEvent @event) {
		if (@event is InputEventMouseMotion mouseMotion) {
			_mouseMovement = mouseMotion.Relative;
		}

		if (@event is InputEventMouseButton mouseButton) {
			if (mouseButton.IsPressed()) {
				if (mouseButton.ButtonIndex == MouseButton.Left) {
					_InteractPressed = true;
				} else if (mouseButton.ButtonIndex == MouseButton.Right) {
					_InteractPressed = false; // Reset on release
				}
			}
		}

		_inputDirection = Input.GetVector("move_left", "move_right", "move_up", "move_down");
		_JumpPressed = Input.IsActionJustPressed("move_jump");
		_CrouchPressed = Input.IsActionPressed("move_crouch");
		_SprintPressed = Input.IsActionPressed("move_sprint");

	}

	public void ResetMouseMovement() {
		_mouseMovement = Vector2.Zero;
	}
}
