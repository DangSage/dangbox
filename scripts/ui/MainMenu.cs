using Godot;

using System;

namespace DangboxGame.Scripts.UI {
	public partial class MainMenu : Control {
		// UI elements
		private Button _startButton;
		private Button _settingsButton;
		private Button _quitButton;
		private bool _isInitialized = false;

		public override void _Ready() {
			// Add a small delay to ensure the scene is fully loaded
			CallDeferred(nameof(InitializeUI));
		}
		
		// Public method that can be called by UIManager to ensure initialization
		public void EnsureInitialized() {
			if (!_isInitialized) {
				InitializeUI();
			}
		}
		
		private void InitializeUI() {
			if (_isInitialized) {
				return;
			}
			
			// Get references to UI elements - updated to use correct paths
			_startButton = GetNodeOrNull<Button>("VerticalContainer/StartButton");
			_settingsButton = GetNodeOrNull<Button>("VerticalContainer/SettingsButton");
			_quitButton = GetNodeOrNull<Button>("VerticalContainer/QuitButton");

			// Connect button signals with error handling
			if (_startButton != null) {
				// Disconnect first to prevent duplicate connections
				if (_startButton.IsConnected("pressed", Callable.From(OnStartButtonPressed))) {
					_startButton.Disconnect("pressed", Callable.From(OnStartButtonPressed));
				}
				_startButton.Connect("pressed", Callable.From(OnStartButtonPressed));
			} else {
				GD.PrintErr("StartButton not found in MainMenu");
			}

			if (_settingsButton != null) {
				if (_settingsButton.IsConnected("pressed", Callable.From(OnSettingsButtonPressed))) {
					_settingsButton.Disconnect("pressed", Callable.From(OnSettingsButtonPressed));
				}
				_settingsButton.Connect("pressed", Callable.From(OnSettingsButtonPressed));
			} else {
				GD.PrintErr("SettingsButton not found in MainMenu");
			}

			if (_quitButton != null) {
				if (_quitButton.IsConnected("pressed", Callable.From(OnQuitButtonPressed))) {
					_quitButton.Disconnect("pressed", Callable.From(OnQuitButtonPressed));
				}
				_quitButton.Connect("pressed", Callable.From(OnQuitButtonPressed));
			} else {
				GD.PrintErr("QuitButton not found in MainMenu");
			}
			
			_isInitialized = true;
		}
		
		private void OnStartButtonPressed() {
			try {
				// Use events instead of directly calling GameManager
				GameEvents.EmitGameStartRequested();
			} catch (Exception e) {
				GD.PrintErr($"Error in OnStartButtonPressed: {e.Message}");
			}
		}

		private void OnSettingsButtonPressed() {
			try {
				UIManager.Instance?.ChangeUIState(UIManager.UIState.SettingsMenu);
			} catch (Exception e) {
				GD.PrintErr($"Error in OnSettingsButtonPressed: {e.Message}");
			}
		}

		private void OnQuitButtonPressed() {
			try {
				GameEvents.EmitQuitGameRequested();
			} catch (Exception e) {
				GD.PrintErr($"Error in OnQuitButtonPressed: {e.Message}");
			}
		}
	}
}
