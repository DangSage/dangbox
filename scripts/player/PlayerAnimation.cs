using Godot;

namespace DangboxGame.Scripts.Player {
	public partial class PlayerAnimation : Node {
		private Skeleton3D _skeleton;
		private PlayerController _playerController;
		private AnimationPlayer _animPlayer;

		private string _currentState = "";

		public override void _Ready() {
			_skeleton = GetNodeOrNull<Skeleton3D>("./Skeleton3D");
			if (_skeleton == null) {
				GD.PrintErr("Skeleton3D node not found!");
				return;
			}
			_playerController = GetNodeOrNull<PlayerController>("../../CharacterBody3D");
			_animPlayer = GetNodeOrNull<AnimationPlayer>("../AnimationPlayer");
			
			if (_playerController == null || _animPlayer == null) {
				GD.PrintErr("PlayerAnimation: Required nodes not found. Scene structure may be incorrect.");
			}
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
				_playerController == null || !IsInstanceValid(_playerController)) {
				return;
			}
			
			int headIndex = _skeleton.FindBone("Head");
			if (headIndex == -1) {
				return; // Don't spam error messages
			}

			// Check if ActualHead node exists and is valid
			var actualHead = _playerController.GetNodeOrNull<Node3D>("../ActualHead");
			if (actualHead == null || !IsInstanceValid(actualHead)) {
				return;
			}

			Transform3D animationPose = _skeleton.GetBonePose(headIndex);
			Quaternion headRotation = new Quaternion(Vector3.Right, actualHead.Rotation.X);

			animationPose.Basis = animationPose.Basis.Slerp(new Basis(headRotation), 0.5f);
			_skeleton.SetBonePose(headIndex, animationPose);
		}

		private void RotateBody(float delta) {
			// Add safety checks
			if (_skeleton == null || !IsInstanceValid(_skeleton) ||
				_playerController == null || !IsInstanceValid(_playerController)) {
				return;
			}
			
			var actualHead = _playerController.GetNodeOrNull<Node3D>("../ActualHead");
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
