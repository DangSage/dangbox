using Godot;

namespace DangboxGame.Scripts.Player {
	public partial class PlayerInput : Node {
		private PlayerController _playerController;
		private Vector2 _inputDirection = Vector2.Zero;
		private Vector2 _mouseMovement = Vector2.Zero;
		private bool _JumpPressed = false;
		private bool _CrouchPressed = false;
		private bool _SprintPressed = false;
		private bool _InteractPressed = false;
		private bool _isPlayerInitialized = false;
        private bool _TakeInput = true;

		public Vector2 InputDirection => _inputDirection;
		public Vector2 MouseMovement => _mouseMovement;
		public bool JumpPressed => _JumpPressed;
		public bool CrouchPressed => _CrouchPressed;
		public bool SprintPressed => _SprintPressed;
		public bool InteractPressed => _InteractPressed;

        public override void _Ready() {
            _playerController = GetParent<PlayerController>();
            _playerController.PlayerInitialized += OnPlayerInitialized;
	
			// Subscribe to input control events
			GameEvents.PlayerInputEnabled += OnPlayerInputEnabled;
            _isPlayerInitialized = false; // Reset state until player is initialized
		}
		
		private void OnPlayerInitialized() {
			_isPlayerInitialized = true;
		}

		private void OnPlayerInputEnabled(bool enabled) {
			_TakeInput = enabled;
	
			// Reset all input states when disabled to prevent stuck inputs
			if (!enabled) {
				_inputDirection = Vector2.Zero;
				_mouseMovement = Vector2.Zero;
				_JumpPressed = false;
				_CrouchPressed = false;
				_SprintPressed = false;
				_InteractPressed = false;
			}
		}

		public override void _Input(InputEvent @event) {
			// Add safety check for disposed objects and input state
			if (!IsInstanceValid(this) || !_TakeInput || !_isPlayerInitialized) {
				return;
			}

			if (@event is InputEventMouseMotion mouseMotion) {
                // GD.Print($"Mouse motion received: {mouseMotion.Relative}");
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
			// Add safety check
			if (!IsInstanceValid(this)) {
				return;
			}
			
			_mouseMovement = Vector2.Zero;
		}

		public override void _ExitTree() {
			// Unsubscribe from events
			GameEvents.PlayerInputEnabled -= OnPlayerInputEnabled;
			base._ExitTree();
		}
	}
}
