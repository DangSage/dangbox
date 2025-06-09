using Godot;
using DangboxGame.Scripts.NBT;

namespace DangboxGame.Scripts.Player {
	public partial class PlayerController : CharacterBody3D {
		[Signal]
		public delegate void PlayerInitializedEventHandler();

		private EntityNBT _playerNbt = new();
		private PlayerInput _playerInput;
		public string activeModifier = "default";

		private Node3D _headTarget;
		private Node3D _cameraTarget;
		public Node3D _actualHead { get; private set; }

		private Camera3D _camera;

		private Vector2 InputDir => _playerInput.InputDirection;
		private Vector2 MouseMovement => _playerInput.MouseMovement;
		private bool JumpPressed => _playerInput.JumpPressed;
		private bool CrouchPressed => _playerInput.CrouchPressed;
		private bool SprintPressed => _playerInput.SprintPressed;


		// safety check stuff
		private bool _nodesValid = false;
		public int _validationFrameCounter = 0;
		private float _cachedAcceleration = -1f;
		private float _cachedGravity = -1f;
		private float _cachedSensitivity = -1f;
		private float _cachedJumpVelocity = -1f;

		public override void _Ready() {
			base._Ready();

			if (!CacheVariables()) {
				GD.PrintErr("PlayerController: Failed to cache initial variables.");
				return;
			}

			InitializePlayer();
		}

		private void InitializePlayer() {
			_headTarget = GetNodeOrNull<Node3D>("_Camera/HeadTarget");
			_cameraTarget = GetNodeOrNull<Node3D>("_Camera/HeadTarget/CameraTarget");
			_actualHead = GetNodeOrNull<Node3D>("_Camera/ActualHead");
			_camera = GetNodeOrNull<Camera3D>("_Camera/ActualHead/Camera");

			_playerInput = GetNodeOrNull<PlayerInput>("_PlayerInput");

			if (_headTarget == null || _cameraTarget == null || _actualHead == null || _playerInput == null) {
				GD.PrintErr("PlayerController: One or more required nodes were NOT loaded.");
				return;
			}

			_camera?.SetProcessInput(true);
			_camera?.SetProcess(true);

			CacheVariables();

			ApplyFOVSetting();
			GameEvents.SettingUpdated += OnSettingUpdated;
			Input.MouseMode = Input.MouseModeEnum.Captured;

			GD.Print("PlayerController initialized successfully");

			// Emit signal when player is fully initialized
			EmitSignal(SignalName.PlayerInitialized);
		}

		public override void _ExitTree() {
			_playerNbt.Serialize("player_data.nbt");
			Input.MouseMode = Input.MouseModeEnum.Visible;

			GameEvents.SettingUpdated -= OnSettingUpdated;

			base._ExitTree();
		}


		public override void _PhysicsProcess(double delta) {
			if (_validationFrameCounter++ % 60 == 0) {
				// Validate nodes every 60 frames
				_nodesValid = ValidateNodes();
				if (!_nodesValid) {
					GD.PrintErr("PlayerController: One or more required nodes are invalid or missing.");
				}

				CacheVariables();
			}

			_actualHead.Position = Position + new Vector3(0, 1.45f, 0);

			HandleMouseMovement();

			// Lerp the entire Rotation vector at once
			Vector3 targetRotation = new Vector3(
				_cameraTarget.Rotation.X,
				_headTarget.Rotation.Y,
				_actualHead.Rotation.Z
			);
			_actualHead.Rotation = targetRotation;

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

			if (IsOnFloor()) {
				Velocity = new Vector3(direction.X * currentSpeed, Velocity.Y, direction.Z * currentSpeed);
				if (JumpPressed) {
					Velocity = new Vector3(Velocity.X, _cachedJumpVelocity, Velocity.Z);
				}
			} else {
				Velocity = new Vector3(
					Mathf.Lerp(Velocity.X, direction.X * currentSpeed, _cachedAcceleration * delta),
					Velocity.Y - _cachedGravity * delta,
					Mathf.Lerp(Velocity.Z, direction.Z * currentSpeed, _cachedAcceleration * delta)
				);
			}
		}

		private void ApplyFOVSetting() {
			if (GameManager.Instance?.Settings != null && _actualHead is Camera3D camera) {
				float fov = GameManager.Instance.Settings.GetFOV();
				camera.Fov = fov;
				GD.Print($"Applied FOV setting: {fov}");
			}
		}

		private void OnSettingUpdated(string settingName, Variant value) {
			if (settingName == "graphics_fov") {
				ApplyFOVSetting();
			}
		}

		private bool ValidateNodes() {
			return _actualHead != null && IsInstanceValid(_actualHead) &&
				_cameraTarget != null && IsInstanceValid(_cameraTarget) &&
				_headTarget != null && IsInstanceValid(_headTarget) &&
				_playerInput != null && IsInstanceValid(_playerInput);
		}

		private bool CacheVariables() {
			_cachedAcceleration = _playerNbt.GetProperty<float>("acceleration");
			_cachedSensitivity = _playerNbt.GetProperty<float>("sensitivity");
			_cachedJumpVelocity = _playerNbt.GetProperty<float>("jump_velocity");
			_cachedGravity = _playerNbt.GetProperty<float>("gravity");

			return _cachedAcceleration >= 0 && _cachedSensitivity >= 0 &&
				   _cachedJumpVelocity >= 0 && _cachedGravity >= 0;

		}
	}
}
