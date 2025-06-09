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
            int headIndex = _skeleton.FindBone("Head");
            if (headIndex == -1) {
                GD.PrintErr("Head bone not found in skeleton");
                return;
            }

            Vector3 headRotation = actualHead.Rotation;
            Transform3D currentGlobalPose = _skeleton.GetBoneGlobalPose(headIndex);

            Vector3 currentEuler = currentGlobalPose.Basis.GetEuler();
            currentEuler.X = headRotation.X; // Only override pitch
            Transform3D newGlobalPose = new(
                Basis.FromEuler(currentEuler),
                currentGlobalPose.Origin // Keep the animated position
            );

            // TODO: Use a more appropriate method for setting bone pose
            // SetBoneGlobalPoseOverride is deprecated, but alternative does not correctly
            // handle overrides without affecting the animation system.
#pragma warning disable CS0618 // Type or member is obsolete
            _skeleton.SetBoneGlobalPoseOverride(headIndex, newGlobalPose, 0.5f, true);
#pragma warning restore CS0618 // Type or member is obsolete
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
