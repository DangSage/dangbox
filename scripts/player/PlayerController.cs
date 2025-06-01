using Godot;
using DangboxGame.Scripts.NBT;

namespace DangboxGame.Scripts.Player {
	public partial class PlayerController : CharacterBody3D {
		private EntityNBT _playerNbt = new();
		private PlayerInput _playerInput;
		public string activeModifier = "default";

		private Node3D _headTarget;
		private Node3D _cameraTarget;
		private Node3D _actualHead;

		private Vector2 InputDir => _playerInput.InputDirection;
		private Vector2 MouseMovement => _playerInput.MouseMovement;
		private bool JumpPressed => _playerInput.JumpPressed;
		private bool CrouchPressed => _playerInput.CrouchPressed;
		private bool SprintPressed => _playerInput.SprintPressed;

		public override void _Ready() {
			// Use safer node access with null checks
			_headTarget = GetNodeOrNull<Node3D>("../_Camera/HeadTarget");
			_cameraTarget = GetNodeOrNull<Node3D>("../_Camera/HeadTarget/CameraTarget");
			_actualHead = GetNodeOrNull<Node3D>("../_Camera/ActualHead");
			_playerInput = GetNodeOrNull<PlayerInput>("../_PlayerInput");
			
			if (_headTarget == null || _cameraTarget == null || _actualHead == null || _playerInput == null) {
				GD.PrintErr("PlayerController: Required nodes not found. Scene structure may be incorrect.");
				return;
			}
			
			ApplyFOVSetting();
			GameEvents.SettingUpdated += OnSettingUpdated;
		}

		public override void _ExitTree() {
			// WRITE NBT DATA TO FILE
			var serial = _playerNbt.Serialize("/user://player_data.nbt");
			Input.MouseMode = Input.MouseModeEnum.Visible; // Restore mouse mode on exit
			
			// Unsubscribe from static events
			GameEvents.SettingUpdated -= OnSettingUpdated;
			
			base._ExitTree();
		}


		public override void _PhysicsProcess(double delta) {
			// Add safety checks for disposed objects
			if (!IsInstanceValid(this) || !IsInsideTree()) {
				return;
			}
			
			if (_actualHead == null || !IsInstanceValid(_actualHead) || 
				_cameraTarget == null || !IsInstanceValid(_cameraTarget) ||
				_headTarget == null || !IsInstanceValid(_headTarget) ||
				_playerInput == null || !IsInstanceValid(_playerInput)) {
				return;
			}

			_actualHead.Position = Position + new Vector3(0, 1.45f, 0);

			HandleMouseMovement();

			float acceleration = _playerNbt.GetProperty<float>("acceleration");

			_actualHead.Rotation = new Vector3(
				Mathf.LerpAngle(_actualHead.Rotation.X, _cameraTarget.Rotation.X, acceleration * 5 * (float)delta),
				Mathf.LerpAngle(_actualHead.Rotation.Y, _headTarget.Rotation.Y, acceleration * 5 * (float)delta),
				_actualHead.Rotation.Z
			);

			Vector3 direction = Vector3.Zero;
			if (InputDir != Vector2.Zero) {
				Basis horizontalBasis = new Basis(Vector3.Up, _actualHead.Rotation.Y);
				direction = (horizontalBasis * new Vector3(InputDir.X, 0, InputDir.Y)).Normalized();
			}

			ApplyVelocity(direction, (float)delta);

			activeModifier = CrouchPressed ? "crouch" :
							 SprintPressed ? "sprint" : "default";

			if (activeModifier == "crouch") {
				_actualHead.Position = new Vector3(_actualHead.Position.X, Mathf.Lerp(_actualHead.Position.Y, Position.Y + 1.25f, 1), _actualHead.Position.Z);
			}

			MoveAndSlide();
		}

		private void HandleMouseMovement() {
			// Add safety checks
			if (_headTarget == null || !IsInstanceValid(_headTarget) ||
				_cameraTarget == null || !IsInstanceValid(_cameraTarget) ||
				_playerInput == null || !IsInstanceValid(_playerInput)) {
				return;
			}
			
			float sensitivity = _playerNbt.GetProperty<float>("sensitivity");
			_headTarget.RotateY(-MouseMovement.X * sensitivity);
			_cameraTarget.RotateX(-MouseMovement.Y * sensitivity);
			_cameraTarget.Rotation = new Vector3(
				Mathf.Clamp(_cameraTarget.Rotation.X, Mathf.DegToRad(-80), Mathf.DegToRad(80)),
				_cameraTarget.Rotation.Y,
				_cameraTarget.Rotation.Z
			);

			_playerInput.ResetMouseMovement();
		}

		private void ApplyVelocity(Vector3 direction, float delta) {
			float currentSpeed = _playerNbt.GetCurrentSpeed(activeModifier);
			float jumpVelocity = _playerNbt.GetProperty<float>("jump_velocity");
			float acceleration = _playerNbt.GetProperty<float>("acceleration");
			float gravity = _playerNbt.GetProperty<float>("gravity");

			if (IsOnFloor()) {
				Velocity = new Vector3(direction.X * currentSpeed, Velocity.Y, direction.Z * currentSpeed);
				if (JumpPressed) {
					Velocity = new Vector3(Velocity.X, jumpVelocity, Velocity.Z);
				}
			} else {
				Velocity = new Vector3(
					Mathf.Lerp(Velocity.X, direction.X * currentSpeed, acceleration * delta),
					Velocity.Y - gravity * delta,
					Mathf.Lerp(Velocity.Z, direction.Z * currentSpeed, acceleration * delta)
				);
			}
		}

		private void ApplyFOVSetting() {
			if (GameSettings.Instance != null) {
				float fov = GameSettings.Instance.GetFOV();
				var camera = GetViewport().GetCamera3D();
				if (camera != null) {
					camera.Fov = fov;
				}
			}
		}
		
		private void OnSettingUpdated(string settingName, Variant value) {
			if (settingName == "graphics_fov") {
				ApplyFOVSetting();
			}
		}
	}
}
