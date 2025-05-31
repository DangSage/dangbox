using Godot;

namespace DangboxGame.Scripts.Player {
	public partial class PlayerAnimation : Node {
		private Skeleton3D _skeleton;
		private PlayerController _playerController;
		private AnimationPlayer _animPlayer;

		private string _currentState = "";

		public override void _Ready() {
			_skeleton = GetNode<Skeleton3D>("./Skeleton3D");
			if (_skeleton == null) {
				GD.PrintErr("Skeleton3D node not found!");
			}
			_playerController = GetNode<PlayerController>("../../CharacterBody3D");
			_animPlayer = GetNode<AnimationPlayer>("../AnimationPlayer");
		}

		public override void _Process(double delta) {
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
			int headIndex = _skeleton.FindBone("Head");
			if (headIndex == -1) {
				GD.PrintErr("Head bone not found in Skeleton3D!");
				return;
			}

			// TODO: headbone doesn't get updated (bad bone pose application? diff method?)
			Transform3D animationPose = _skeleton.GetBonePose(headIndex);
			Node3D actualHead = _playerController.GetNode<Node3D>("../ActualHead");
			Quaternion headRotation = new Quaternion(Vector3.Right, actualHead.Rotation.X);

			animationPose.Basis = animationPose.Basis.Slerp(new Basis(headRotation), 0.5f);
			_skeleton.SetBonePose(headIndex, animationPose);
		}

		private void RotateBody(float delta) {
			float targetRotationY = _playerController.GetNode<Node3D>("../ActualHead").Rotation.Y;
			float currentRotationY = _skeleton.Rotation.Y;
			_skeleton.Rotation = new Vector3(
				_skeleton.Rotation.X,
				Mathf.LerpAngle(currentRotationY, targetRotationY, delta * 10),
				_skeleton.Rotation.Z
			);
		}
	}
}
