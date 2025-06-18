using Godot;
using DangboxGame.Scripts.NBT;

namespace DangboxGame.Scripts.Player {
	public partial class PlayerController : CharacterBody3D {
		[Signal]
		public delegate void PlayerInitializedEventHandler();

		private EntityNBT _playerNbt = new();
		private PlayerInput _playerInput;
		private PlayerAnimation _playerAnimation;
		public string activeModifier = "default";

		private Node3D _headTarget;
		private Node3D _cameraTarget;
		public Node3D _actualHead { get; private set; }
		public Camera3D _camera;

		// Camera mode state
		private bool _isFirstPerson = true;
		public bool IsFirstPerson => _isFirstPerson;

		private Vector2 InputDir => _playerInput.InputDirection;
		private Vector3 Direction { get; set; } = Vector3.Zero;
		private Vector2 MouseMovement => _playerInput.MouseMovement;
		private bool JumpPressed => _playerInput.JumpPressed;
		private bool CrouchPressed => _playerInput.CrouchPressed;
		private bool SprintPressed => _playerInput.SprintPressed;
		private bool CameraSwitchPressed => _playerInput.CameraSwitchPressed;


		// safety check stuff
		private bool _nodesValid = false;
		private float _acceleration = -1f;
		private float _gravity = -1f;
		private float _jumpVelocity = -1f;
		private float _sensitivity = 0.33f;  // Changed from 0.1f to 0.33f

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
			_playerAnimation = GetNodeOrNull<PlayerAnimation>("Model/PlayerAnimation");

			if (_headTarget == null || _cameraTarget == null || _actualHead == null || _playerInput == null) {
				GD.PrintErr("PlayerController: One or more required nodes were NOT loaded.");
				return;
			}

			ApplyFOVSetting();
			ApplySensitivitySetting();
			CacheVariables();
			GameEvents.SettingUpdated += OnSettingUpdated;
			Input.MouseMode = Input.MouseModeEnum.Captured;

			// Set initial camera mode
			SetFirstPersonMode(_isFirstPerson);

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
			// Handle camera switching
			if (CameraSwitchPressed) {
				ToggleCameraMode();
			}

			ProcessCamera();
			ProcessMovement((float)delta);

			activeModifier = CrouchPressed ? "crouch" :
							 SprintPressed ? "sprint" : "default";

			if (activeModifier == "crouch") {
				_actualHead.Position = Position + new Vector3(0, 1.0f, 0);
			} else {
				_actualHead.Position = Position + new Vector3(0, 1.25f, 0);
			}

			MoveAndSlide();
		}

		private void ToggleCameraMode() {
			_isFirstPerson = !_isFirstPerson;
			SetFirstPersonMode(_isFirstPerson);
		}

		private void SetFirstPersonMode(bool firstPerson) {
			_isFirstPerson = firstPerson;
			
			// Update animation system
			_playerAnimation?.SetFirstPersonMode(_isFirstPerson);
			
			// Future: Add third-person camera positioning logic here
			if (!_isFirstPerson) {
				// TODO: Position camera behind player for third-person
			}
		}

		private void ProcessCamera() {
			_headTarget.RotateY(-MouseMovement.X * (_sensitivity*0.01f));
			_cameraTarget.RotateX(-MouseMovement.Y * (_sensitivity*0.01f));
			_cameraTarget.Rotation = new Vector3(
				Mathf.Clamp(_cameraTarget.Rotation.X, Mathf.DegToRad(-80), Mathf.DegToRad(80)),
				_cameraTarget.Rotation.Y,
				_cameraTarget.Rotation.Z
			);

			Vector3 targetRotation = new Vector3(
				_cameraTarget.Rotation.X,
				_headTarget.Rotation.Y,
				_actualHead.Rotation.Z
			);
			_actualHead.Rotation = targetRotation;

			if (InputDir != Vector2.Zero) {
				Basis horizontalBasis = new Basis(Vector3.Up, _actualHead.Rotation.Y);
				Direction = (horizontalBasis * new Vector3(InputDir.X, 0, InputDir.Y)).Normalized();
			} else {
				Direction = Vector3.Zero;
			}
		}

		private void ProcessMovement(float delta) {
			float currentSpeed = _playerNbt.GetCurrentSpeed(activeModifier);

			if (IsOnFloor()) {
				Velocity = new Vector3(Direction.X * currentSpeed, Velocity.Y, Direction.Z * currentSpeed);
				if (JumpPressed) {
					Velocity = new Vector3(Velocity.X, _jumpVelocity, Velocity.Z);
				}
				_playerInput.ResetMouseMovement();
			} else {
				Velocity = new Vector3(
					Mathf.Lerp(Velocity.X, Direction.X * currentSpeed, _acceleration * delta),
					Velocity.Y - _gravity * delta,
					Mathf.Lerp(Velocity.Z, Direction.Z * currentSpeed, _acceleration * delta)
				);
			}
		}

		private void ApplyFOVSetting() {
			if (GameManager.Instance?.Settings != null && _camera != null) {
				float fov = GameManager.Instance.Settings.GetFOV();
				_camera.Fov = fov;
			}
		}

		private void ApplySensitivitySetting() {
			if (GameManager.Instance?.Settings != null) {
				_sensitivity = GameManager.Instance.Settings.GetSensitivity();
				GD.Print($"PlayerController: Sensitivity set to {_sensitivity}");
			}
		}

		private void OnSettingUpdated(string settingName, Variant value) {
			switch (settingName) {
				case "input_sensitivity":
					_sensitivity = value.AsSingle();
					break;
				case "graphics_fov":
					ApplyFOVSetting();
					break;
			}
		}

		private bool ValidateNodes() {
			return _actualHead != null && IsInstanceValid(_actualHead) &&
				_cameraTarget != null && IsInstanceValid(_cameraTarget) &&
				_headTarget != null && IsInstanceValid(_headTarget) &&
				_playerInput != null && IsInstanceValid(_playerInput);
		}

		private bool CacheVariables() {
			_acceleration = _playerNbt.GetProperty<float>("acceleration");
			_sensitivity = _playerNbt.GetProperty<float>("sensitivity");
			_jumpVelocity = _playerNbt.GetProperty<float>("jump_velocity");
			_gravity = _playerNbt.GetProperty<float>("gravity");

			return _acceleration >= 0 && _sensitivity >= 0 &&
				   _jumpVelocity >= 0 && _gravity >= 0;

		}
	}
}
