using Godot;

namespace DangboxGame.Scripts.Player {
	public partial class PlayerAnimation : Node {
		private Skeleton3D _skeleton;
		private PlayerController _playerController;
		private Node3D actualHead;
		private AnimationPlayer _animPlayer;

		private string _currentState = "";
		private bool _isInitialized = false;

		public override void _Ready() {
			_skeleton = GetNodeOrNull<Skeleton3D>("Skeleton3D");
			_playerController = GetNodeOrNull<PlayerController>("../../CharacterBody3D");
			_animPlayer = GetNodeOrNull<AnimationPlayer>("../AnimationPlayer");
			_playerController.PlayerInitialized += OnPlayerInitialized;
		}

		private void OnPlayerInitialized() {
			if (_playerController == null || _animPlayer == null || _skeleton == null) {
				GD.PrintErr("PlayerAnimation: Required nodes not found. Scene structure may be incorrect.");
				return;
			}

			actualHead = _playerController._actualHead;
			if (actualHead == null || !IsInstanceValid(actualHead)) {
				GD.PrintErr("PlayerAnimation: Actual head node not found or invalid.");
			} else {
				GD.Print("PlayerAnimation: Successfully found ActualHead node");
			}
			_isInitialized = true;
		}

		public override void _Process(double delta) {
			// Add safety checks for disposed objects
			if (!IsInstanceValid(this) || !IsInsideTree()) {
				return;
			}

			if (_playerController == null || !IsInstanceValid(_playerController) ||
				_skeleton == null || !IsInstanceValid(_skeleton) ||
				_animPlayer == null || !IsInstanceValid(_animPlayer)) {
				return;
			}

			string activeModifier = _playerController.activeModifier;

			PlayAnimation(activeModifier);
			RotateBody((float)delta);
			UpdateHeadBone();
		}

		private void PlayAnimation(string activeModifier) {
			if (!_isInitialized || _animPlayer == null) {
				return;
			}
			
			Vector3 velocity = _playerController.Velocity;
			bool isMoving = Mathf.Abs(velocity.X) > 0.1f || Mathf.Abs(velocity.Z) > 0.1f;
			float blendTime;
			string targetAnimation;

			targetAnimation = activeModifier == "crouch" 
				? (isMoving ? "sneak-move" : "sneak") 
				: (isMoving ? "move" : "idle");

			blendTime = (_currentState.StartsWith("sneak") != targetAnimation.StartsWith("sneak")) 
				? 0 
				: (isMoving ? 0.2f : 0.1f);

			if (_currentState != targetAnimation) {
				_currentState = targetAnimation;
				_animPlayer.Play(targetAnimation, blendTime);
			}

			float speedScale = velocity.Length() / 4;
			_animPlayer.SpeedScale = Mathf.Clamp(speedScale, 0.5f, 2.0f);
		}

		private void UpdateHeadBone() {
			// Add safety checks
			if (_skeleton == null || !IsInstanceValid(_skeleton) ||
				_playerController == null || !IsInstanceValid(_playerController) ||
				actualHead == null || !IsInstanceValid(actualHead)) {
				return;
			}

			// Instead of setting the bone pose, use SetBoneGlobalPoseOverride for runtime control
			Transform3D headGlobalTransform = actualHead.GlobalTransform;
			Basis headBasis = headGlobalTransform.Basis;
			Vector3 headEuler = headBasis.GetEuler();
			// Only apply X (pitch) rotation, keep other axes unchanged
			int headIndex = _skeleton.FindBone("Head");
			Transform3D boneGlobalTransform = _skeleton.GetBoneGlobalPose(headIndex);

			Basis newBasis = boneGlobalTransform.Basis;
			Vector3 boneEuler = newBasis.GetEuler();
			boneEuler.X = headEuler.X; // Only set X rotation
			newBasis = Basis.FromEuler(boneEuler);

			Transform3D newTransform = boneGlobalTransform;
			newTransform.Basis = newBasis;

			_skeleton.SetBoneGlobalPoseOverride(
				headIndex,
				newTransform,
				1.0f, // full weight
				true  // persistent
			);
		}

		private void RotateBody(float delta) {
			// Add safety checks
			if (_skeleton == null || !IsInstanceValid(_skeleton) ||
				_playerController == null || !IsInstanceValid(_playerController)) {
				return;
			}

			if (actualHead == null || !IsInstanceValid(actualHead)) {
				return;
			}
			
			float targetRotationY = actualHead.Rotation.Y;
			float currentRotationY = _skeleton.Rotation.Y;
			_skeleton.Rotation = new Vector3(
				_skeleton.Rotation.X,
				Mathf.LerpAngle(currentRotationY, targetRotationY, delta * 10),
				_skeleton.Rotation.Z
			);
		}
	}
}
