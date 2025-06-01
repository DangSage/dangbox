// Entry point for Dangbox, needs to load up necessary assets + mainscreen instance
// Verify some client-side settings and preferences as well

using Godot;
using System.Collections.Generic;
using DangboxGame.Scripts.UI;

namespace DangboxGame.Scripts {
	public partial class GameManager : Node {
		public static GameManager Instance { get; private set; }

		private Node _currentScene;
		private Node _playerInstance;
		private Node _cameraInstance;

		// Track resources to verify at startup
		private readonly Dictionary<string, bool> _resourceVerification = [];

		public override void _Ready() {
			if (Instance != null) {
				QueueFree();
				return;
			}
			Instance = this;

			VerifyResources();

			// Ensure UIManager is added first
			var uiManager = UIManager.Instance;
			if (uiManager != null) {
				AddChild(uiManager);
			} else {
				GD.PrintErr("Failed to load UIManager scene.");
				return;
			}

			// Initialize UIManager and connect signals
			UIManager.Instance.ChangeUIState(UIManager.UIState.MainMenu);
			ConnectUISignals();
		}

		private void VerifyResources() {
			// Add all critical resources to verify
			_resourceVerification.Add(Constants.ScenePath.MainMenu, ResourceExists(Constants.ScenePath.MainMenu));
			_resourceVerification.Add(Constants.ScenePath.TestLevel, ResourceExists(Constants.ScenePath.TestLevel));
			_resourceVerification.Add(Constants.ScenePath.SettingsMenu, ResourceExists(Constants.ScenePath.SettingsMenu));
			_resourceVerification.Add(Constants.ScenePath.PauseMenu, ResourceExists(Constants.ScenePath.PauseMenu));
			_resourceVerification.Add(Constants.PrefabPath.Player, ResourceExists(Constants.PrefabPath.Player));
			_resourceVerification.Add(Constants.PrefabPath.Camera, ResourceExists(Constants.PrefabPath.Camera));
			_resourceVerification.Add(Constants.PrefabPath.HUD, ResourceExists(Constants.PrefabPath.HUD));
			_resourceVerification.Add(Constants.ScriptPath.PlayerInput, ResourceExists(Constants.ScriptPath.PlayerInput));

			// Report any missing resources
			bool allResourcesValid = true;
			foreach (var resource in _resourceVerification) {
				if (!resource.Value) {
					GD.PrintErr($"Critical resource missing: {resource.Key}");
					allResourcesValid = false;
				}
			}

			if (allResourcesValid) {
				GD.Print("All critical resources verified successfully.");
			} else {
				GD.PrintErr("Some critical resources are missing. Game may not function correctly.");
			}
		}

		private bool ResourceExists(string path) {
			return ResourceLoader.Exists(path);
		}

		private void ConnectUISignals() {
			// Wait a frame to ensure UIManager has created the UI
			CallDeferred(nameof(DelayedConnectSignals));
		}

		private void DelayedConnectSignals() {
			var mainMenu = UIManager.Instance.GetCurrentState() == UIManager.UIState.MainMenu
				? UIManager.Instance.GetNodeOrNull<MainMenu>("MenuLayer/MainMenu")
				: null;

			if (mainMenu != null) {
				mainMenu.GetNode<Button>("StartButton").Connect("pressed", Callable.From(StartHostGame));
				mainMenu.GetNode<Button>("SettingsButton").Connect("pressed", Callable.From(OnSettingsPressed));
				mainMenu.GetNode<Button>("QuitButton").Connect("pressed", Callable.From(OnQuitButtonPressed));
			} else {
				GD.PrintErr("Failed to connect UI signals: Main menu not found");
			}
		}

		private void LoadScene(string scenePath) {
			if (!_resourceVerification.GetValueOrDefault(scenePath, false)) {
				GD.PrintErr($"Cannot load scene: {scenePath} - Resource verification failed");
				return;
			}

			// Remove the current scene if it exists
			if (_currentScene != null) {
				_currentScene.QueueFree();
				_currentScene = null;
			}

			// Load and instantiate the new scene
			var scene = GD.Load<PackedScene>(scenePath);
			if (scene != null) {
				_currentScene = scene.Instantiate();
				AddChild(_currentScene);
			} else {
				GD.PrintErr($"Failed to load scene: {scenePath}");
			}
		}

		public void StartHostGame() {
			LoadScene(Constants.ScenePath.TestLevel);
			SetupPlayer();
		}

		private void SetupPlayer() {
			// Create player
			_playerInstance = InstantiateNode(Constants.PrefabPath.Player);
			if (_playerInstance != null) {
				AddChild(_playerInstance);

				// Add PlayerInput node dynamically
				var playerInput = new Node { Name = "_PlayerInput" };
				var inputScript = GD.Load<Script>(Constants.ScriptPath.PlayerInput);
				if (inputScript != null) {
					playerInput.SetScript(inputScript);
					_playerInstance.AddChild(playerInput);
				} else {
					GD.PrintErr("Failed to load PlayerInput script");
				}
			} else {
				GD.PrintErr("Failed to instantiate player");
			}

			// Create camera
			_cameraInstance = InstantiateNode(Constants.PrefabPath.Camera);
			if (_cameraInstance != null) {
				AddChild(_cameraInstance);
			} else {
				GD.PrintErr("Failed to instantiate camera");
			}
		}

		private void OnSettingsPressed() {
			UIManager.Instance.ChangeUIState(UIManager.UIState.SettingsMenu);
		}

		private void OnQuitButtonPressed() {
			GetTree().Quit();
		}

		private Node InstantiateNode(string resourcePath) {
			if (!_resourceVerification.GetValueOrDefault(resourcePath, false)) {
				GD.PrintErr($"Cannot instantiate: {resourcePath} - Resource verification failed");
				return null;
			}

			var packedScene = GD.Load<PackedScene>(resourcePath);
			if (packedScene != null) {
				return packedScene.Instantiate();
			} else {
				GD.PrintErr($"Failed to load resource: {resourcePath}");
				return null;
			}
		}
	}
}
