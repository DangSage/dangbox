using Godot;
using System;
using System.Collections.Generic;
using DangboxGame.Scripts.UI;

namespace DangboxGame.Scripts {
	public partial class GameResourceVerifier : Node {
		private readonly Dictionary<string, bool> _resourceVerification = new();
		private bool _isVerifying = false;
		private float _verificationProgress = 0f;

		private LoadingScreen _loadingScreen;

		[Export]
		public bool AutoStart { get; set; } = true;

		public override void _Ready() {
			_loadingScreen = GetNode<LoadingScreen>("VBoxContainer");

			if (AutoStart) {
				CallDeferred(nameof(StartResourceVerification));
			}
		}

		public void StartResourceVerification() {
			if (_isVerifying) return;

			_isVerifying = true;
			_verificationProgress = 0f;
			_loadingScreen?.StartLoading("Verifying resources...");

			VerifyResources();
		}

		private async void VerifyResources() {
			try {
				InitializeResourceList();

				int totalResources = _resourceVerification.Count;
				int verifiedResources = 0;
				bool hasErrors = false;

				foreach (var key in new List<string>(_resourceVerification.Keys)) {
					bool exists = ResourceLoader.Exists(key);
					_resourceVerification[key] = exists;

					verifiedResources++;
					_verificationProgress = (float)verifiedResources / totalResources;

					// Update loading screen progress
					_loadingScreen?.UpdateProgress(_verificationProgress, $"Verifying: {System.IO.Path.GetFileName(key)}");

					if (!exists) {
						GD.PrintErr($"Critical resource missing: {key}");
						hasErrors = true;
					}

					await ToSignal(GetTree().CreateTimer(0.03), Timer.SignalName.Timeout);
				}

				_isVerifying = false;

				if (hasErrors) {
					_loadingScreen?.ShowError("Critical resources missing! Check console for details.");
					await ToSignal(GetTree().CreateTimer(3.0), Timer.SignalName.Timeout);
					GetTree().Quit();
					return;
				}

				_loadingScreen?.CompleteLoading("Verification complete!");
				await ToSignal(GetTree().CreateTimer(0.5), Timer.SignalName.Timeout);
				ProceedToGame();
			} catch (Exception e) {
				GD.PrintErr($"Error verifying resources: {e.Message}");
				_isVerifying = false;
				_loadingScreen?.ShowError($"Verification failed: {e.Message}");
				await ToSignal(GetTree().CreateTimer(3.0), Timer.SignalName.Timeout);
				GetTree().Quit();
			}
		}

		private void InitializeResourceList() {
			_resourceVerification.Clear();

			// Use constants for consistency
			_resourceVerification.Add(Constants.ScenePath.MainMenu, false);
			_resourceVerification.Add(Constants.ScenePath.TestLevel, false);
			_resourceVerification.Add(Constants.ScenePath.SettingsMenu, false);
			_resourceVerification.Add(Constants.ScenePath.PauseMenu, false);
			_resourceVerification.Add(Constants.PrefabPath.Player, false);
			_resourceVerification.Add(Constants.PrefabPath.Camera, false);
			_resourceVerification.Add(Constants.PrefabPath.HUD, false);
			_resourceVerification.Add(Constants.ScriptPath.PlayerInput, false);
		}

		private void ProceedToGame() {
			try {
				if (UIManager.Instance == null || GameManager.Instance == null) {
					CallDeferred(nameof(ProceedToGame));
					return;
				}
				
				GameManager.Instance.InitializeGameState(GameManager.GameState.MainMenu);
				CallDeferred(nameof(SafeCleanup));
			} catch (Exception e) {
				GD.PrintErr($"Error in ProceedToGame: {e.Message}");
				CallDeferred(nameof(SafeCleanup));
			}
		}

		private void SafeCleanup() {
			try {
				if (IsInsideTree()) {
					GetParent()?.RemoveChild(this);
				}
				QueueFree();
			} catch (Exception e) {
				GD.PrintErr($"Error in SafeCleanup: {e.Message}");
			}
		}
	}
}
