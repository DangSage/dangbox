using Godot;
using System;

namespace DangboxGame.Scripts.UI {
	public partial class PauseMenu : Control {
		// UI elements
		private Button _resumeButton;
		private Button _settingsButton;
		private Button _mainMenuButton;
		private Button _quitButton;

		public override void _Ready() {
			// Get references to UI elements
			_resumeButton = GetNode<Button>("VBoxContainer/ResumeButton");
			_settingsButton = GetNode<Button>("VBoxContainer/SettingsButton");
			_mainMenuButton = GetNode<Button>("VBoxContainer/MainMenuButton");
			_quitButton = GetNode<Button>("VBoxContainer/QuitButton");

			// Connect button signals
			if (_resumeButton != null) {
				_resumeButton.Connect("pressed", Callable.From(OnResumeButtonPressed));
			} else {
				GD.PrintErr("ResumeButton not found in PauseMenu");
			}

			if (_settingsButton != null) {
				_settingsButton.Connect("pressed", Callable.From(OnSettingsButtonPressed));
			} else {
				GD.PrintErr("SettingsButton not found in PauseMenu");
			}

			if (_mainMenuButton != null) {
				_mainMenuButton.Connect("pressed", Callable.From(OnMainMenuButtonPressed));
			} else {
				GD.PrintErr("MainMenuButton not found in PauseMenu");
			}

			if (_quitButton != null) {
				_quitButton.Connect("pressed", Callable.From(OnQuitButtonPressed));
			} else {
				GD.PrintErr("QuitButton not found in PauseMenu");
			}
		}

		private void OnResumeButtonPressed() {
			try {
				if (UIManager.Instance != null) {
					UIManager.Instance.ResumeGame();
				}
			} catch (Exception e) {
				GD.PrintErr($"Error in OnResumeButtonPressed: {e.Message}");
			}
		}

		private void OnSettingsButtonPressed() {
			try {
				if (UIManager.Instance != null) {
					UIManager.Instance.ChangeUIState(UIManager.UIState.SettingsMenu);
				}
			} catch (Exception e) {
				GD.PrintErr($"Error in OnSettingsButtonPressed: {e.Message}");
			}
		}

		private void OnMainMenuButtonPressed() {
			try {
				if (UIManager.Instance != null) {
					UIManager.Instance.RestartToStartMenu();
				}
			} catch (Exception e) {
				GD.PrintErr($"Error in OnMainMenuButtonPressed: {e.Message}");
			}
		}

		private void OnQuitButtonPressed() {
			try {
				GetTree().Quit();
			} catch (Exception e) {
				GD.PrintErr($"Error in OnQuitButtonPressed: {e.Message}");
			}
		}

		// Handle the Escape key for resume
		public override void _Input(InputEvent @event) {
			if (@event.IsActionPressed("ui_cancel") && UIManager.Instance != null && UIManager.Instance.IsPaused()) {
				OnResumeButtonPressed();
				GetViewport().SetInputAsHandled();
			}
		}
	}
}
