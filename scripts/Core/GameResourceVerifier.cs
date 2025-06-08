using Godot;
using System;
using System.Collections.Generic;
using DangboxGame.Scripts.UI;
using DangboxGame.Scripts.Core.Environment;

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
			_loadingScreen?.StartLoading("Initializing environment...");

			VerifyResources();
		}

		private async void VerifyResources() {
			try {
				// First, wait for EnvironmentConfig to be ready
				await WaitForEnvironmentConfig();
				
				_loadingScreen?.UpdateProgress(0.1f, "Environment configuration loaded");
				
				// Verify EnvironmentConfig is working properly
				VerifyEnvironmentConfig();
				
				_loadingScreen?.UpdateProgress(0.2f, "Verifying game resources...");
				
				InitializeResourceList();

				int totalResources = _resourceVerification.Count;
				int verifiedResources = 0;
				bool hasErrors = false;

				foreach (var key in new List<string>(_resourceVerification.Keys)) {
					bool exists = ResourceLoader.Exists(key);
					_resourceVerification[key] = exists;

					verifiedResources++;
					_verificationProgress = 0.2f + (0.8f * verifiedResources / totalResources);

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

		private async System.Threading.Tasks.Task WaitForEnvironmentConfig() {
			int maxWaitTime = 100; // 10 seconds max wait
			int waitCount = 0;
			
			while (EnvironmentConfig.Instance == null && waitCount < maxWaitTime) {
				await ToSignal(GetTree().CreateTimer(0.1), Timer.SignalName.Timeout);
				waitCount++;
			}
			
			if (EnvironmentConfig.Instance == null) {
				throw new InvalidOperationException("EnvironmentConfig failed to initialize within timeout period");
			}
			
			GD.Print($"EnvironmentConfig ready after {waitCount * 0.1f} seconds");
		}

		private void VerifyEnvironmentConfig() {
			var envConfig = EnvironmentConfig.Instance;
			if (envConfig == null) {
				throw new InvalidOperationException("EnvironmentConfig instance is null");
			}

			// Verify environment detection
			var currentEnv = envConfig.GetCurrentEnvironment();
			GD.Print($"Current environment: {currentEnv}");

			// Verify config path
			string configPath = envConfig.GetConfigPath();
			GD.Print($"Config path: {configPath}");

			// Test some configuration values
			string testScenePath = envConfig.GetValue<string>("scene_main_menu", "");
			string testDataPath = envConfig.GetValue<string>("data_saves", "");
			bool debugEnabled = envConfig.GetValue<bool>("debug_enabled", false);

			GD.Print($"Sample config values - Scene: {testScenePath}, Data: {testDataPath}, Debug: {debugEnabled}");

			if (string.IsNullOrEmpty(testScenePath)) {
				throw new InvalidOperationException("EnvironmentConfig failed to provide valid scene path");
			}
		}

		private void InitializeResourceList() {
			_resourceVerification.Clear();

			var envConfig = EnvironmentConfig.Instance;
			if (envConfig == null) {
				throw new InvalidOperationException("EnvironmentConfig not available during resource list initialization");
			}

			// Use EnvironmentConfig for all resource paths
			_resourceVerification.Add(envConfig.GetValue<string>("scene_main_menu"), false);
			_resourceVerification.Add(envConfig.GetValue<string>("scene_test_level"), false);
			_resourceVerification.Add(envConfig.GetValue<string>("scene_settings_menu"), false);
			_resourceVerification.Add(envConfig.GetValue<string>("scene_pause_menu"), false);
			_resourceVerification.Add(envConfig.GetValue<string>("prefab_player"), false);
			_resourceVerification.Add(envConfig.GetValue<string>("prefab_camera"), false);
			_resourceVerification.Add(envConfig.GetValue<string>("prefab_hud"), false);
			_resourceVerification.Add(envConfig.GetValue<string>("script_player_input"), false);

			GD.Print($"Initialized resource verification list with {_resourceVerification.Count} items from EnvironmentConfig");
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
