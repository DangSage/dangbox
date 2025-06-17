using Godot;
using System.Collections.Generic;

namespace DangboxGame.Scripts.Player {
	public partial class PlayerAnimation : Node {
		private Skeleton3D _skeleton;
		private readonly Dictionary<string, int> _boneCache = new();
		private PlayerController _playerController;
		private Node3D actualHead;
		private AnimationPlayer _animPlayer;

		private MeshInstance3D _skinMesh;
		private MeshInstance3D _armorMesh;

		[Export]
		private ShaderMaterial _headHidingMaterial;

		private string _currentState = "";
		private bool _isInitialized = false;

		public override void _Ready() {
			_skeleton = GetNodeOrNull<Skeleton3D>("Skeleton3D");
			_playerController = GetNodeOrNull<PlayerController>("../../CharacterBody3D");
			_animPlayer = GetNodeOrNull<AnimationPlayer>("../AnimationPlayer");
			_skinMesh = GetNodeOrNull<MeshInstance3D>("Skin/MeshInstance3D");
			_armorMesh = GetNodeOrNull<MeshInstance3D>("Armor/MeshInstance3D");
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
			
			_boneCache["Head"] = _skeleton.FindBone("Head");
			SetupMeshHiding();  // Initialize the head hiding system
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

		[System.Obsolete]
		private void UpdateHeadBone() {
			Vector3 headRotation = actualHead.Rotation;
			Transform3D currentGlobalPose = _skeleton.GetBoneGlobalPose(_boneCache["Head"]);

			Quaternion currentQuat = currentGlobalPose.Basis.GetRotationQuaternion();
			Quaternion pitchQuat = new Quaternion(Vector3.Right, headRotation.X);
			Quaternion newQuat = pitchQuat * currentQuat;

			Transform3D newGlobalPose = new Transform3D(
				new Basis(newQuat),
				currentGlobalPose.Origin // Keep the animated position
			);

			// TODO: Use a more appropriate method for setting bone pose
			// _skeleton.SetBonePose(headIndex, newGlobalPose);
			_skeleton.SetBoneGlobalPoseOverride(_boneCache["Head"], newGlobalPose, 0.5f, true);
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

		private void SetupMeshHiding() {
			if (_skinMesh != null && _boneCache.ContainsKey("Head") && _boneCache["Head"] != -1) {
				// Load the head hiding shader
				Shader headHidingShader = GD.Load<Shader>("res://shaders/HeadHidingShader.gdshader");
				_headHidingMaterial = new ShaderMaterial {
					Shader = headHidingShader
				};

				// Set shader parameters
				_headHidingMaterial.SetShaderParameter("head_bone_index", _boneCache["Head"]);

				// Apply to mesh
				_skinMesh.MaterialOverride = _headHidingMaterial;
			}
		}

		public void SetFirstPersonMode(bool isFirstPerson) {
			if (_headHidingMaterial != null) {
				_headHidingMaterial.SetShaderParameter("hide_head_for_fp", isFirstPerson);
			}
		}
	}
}
