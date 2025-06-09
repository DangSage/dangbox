using Godot;
using System;
using System.Collections.Generic;

namespace DangboxGame.Scripts.UI {
	public partial class UIManager : CanvasLayer {
		public static UIManager Instance { get; private set; }

		public enum UIState {
			None,
			MainMenu,
			SettingsMenu,
			PauseMenu,
			HUD,
			LoadingScreen
		}

		private UIState _currentState = UIState.None;
		private readonly Dictionary<UIState, Control> _uiInstances = [];
		private readonly Dictionary<UIState, string> _uiPaths = new() {
			{ UIState.MainMenu, Constants.ScenePath.MainMenu },
			{ UIState.SettingsMenu, Constants.ScenePath.SettingsMenu },
			{ UIState.PauseMenu, Constants.ScenePath.PauseMenu },
			{ UIState.HUD, Constants.PrefabPath.HUD },
			{ UIState.LoadingScreen, "res://scenes/ui/LoadingScreen.tscn" }
		};

		private bool _isPaused = false;

		// Background elements
		private ColorRect _mainBG;
		private ColorRect _opacityBG;

		private ColorRect CreateFullRectBackground(Color color)
		{
			var rect = new ColorRect
			{
			Color = color,
			Name = "GeneratedBG",
			AnchorLeft = 0,
			AnchorTop = 0,
			AnchorRight = 1,
			AnchorBottom = 1,
			OffsetLeft = 0,
			OffsetTop = 0,
			OffsetRight = 0,
			OffsetBottom = 0
			};
			rect.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
			return rect;
		}

		public override void _Ready() {
			if (Instance != null && Instance != this) {
				GD.PrintErr("Duplicate UIManager found - this should not happen with autoload");
				QueueFree();
				return;
			}
			Instance = this;
			ProcessMode = ProcessModeEnum.Always;
			Layer = 100;
			
			var root = GetTree().Root;
			if (GetParent() != root) {
				GetParent()?.RemoveChild(this);
				root.AddChild(this);
			}
			
			// Create and add background elements as children
			_mainBG = CreateFullRectBackground(new Color(0.25f, 0.25f, 0.25f, 1.0f));
			_mainBG.Name = "MainBG";
			_mainBG.Visible = false;
			AddChild(_mainBG);
			
			_opacityBG = CreateFullRectBackground(new Color(0f, 0f, 0f, 0.8f));
			_opacityBG.Name = "OpacityBG";
			_opacityBG.Visible = false;
			AddChild(_opacityBG);
			
			// Ensure backgrounds are behind UI elements
			_mainBG.ZIndex = -100;
			_opacityBG.ZIndex = -90;
			
			// Subscribe to static events
			GameEvents.UIStateChanged += OnUIStateChanged;
			GameEvents.SettingUpdated += OnSettingUpdated;
			
			// Initialize background visibility
			UpdateBackgroundVisibility();
		}

		public override void _Input(InputEvent @event) {
			if (@event is InputEventKey keyEvent && keyEvent.IsActionPressed("ui_cancel")) {
				HandleEscapeKey();
			}
		}

		private void HandleEscapeKey() {
			switch (_currentState) {
				case UIState.SettingsMenu:
					// Check context to determine where to go back
					if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameManager.GameState.InGame) {
						ChangeUIState(UIState.HUD);
					} else {
						ChangeUIState(UIState.MainMenu);
					}
					break;
				case UIState.HUD:   // In-game, whether in a interaction or not
					GameEvents.EmitGamePauseRequested();
					break;
				case UIState.PauseMenu:
					GameEvents.EmitGameResumeRequested();
					break;
			}
		}

		private void OnUIStateChanged(string state) {
			switch (state) {
				case "main_menu":
					ChangeUIState(UIState.MainMenu);
					break;
				case "settings":
					ChangeUIState(UIState.SettingsMenu);
					break;
				case "hud":
					ChangeUIState(UIState.HUD);
					break;
				case "paused":
					ChangeUIState(UIState.PauseMenu);
					break;
				case "loading":
					ChangeUIState(UIState.LoadingScreen);
					break;
			}
		}

		public void RestartToStartMenu() {
			GD.Print("Restarting to Start menu...");
			
			// Clean up only UI instances, not the UIManager itself
			foreach (var kvp in _uiInstances) {
				if (kvp.Value != null && IsInstanceValid(kvp.Value)) {
					kvp.Value.QueueFree();
				}
			}
			_uiInstances.Clear();
			
			// Reset state but keep UIManager alive
			_currentState = UIState.None;
			_isPaused = false;
			// Re-enable player input before scene change
			GameEvents.EmitPlayerInputEnabled(true);
			Input.MouseMode = Input.MouseModeEnum.Visible;
			
			// Hide backgrounds during restart
			if (_mainBG != null) _mainBG.Visible = false;
			if (_opacityBG != null) _opacityBG.Visible = false;
			
			// Load the Start scene
			GetTree().ChangeSceneToFile("res://scenes/Start.tscn");
		}

		public void ChangeUIState(UIState newState) {
			if (_currentState == newState) return;

			// Simplified: only restart when explicitly going from game to main menu
			if (_currentState == UIState.HUD && newState == UIState.MainMenu) {
				RestartToStartMenu();
				return;
			}

			// Clean up UI instances only when necessary
			if (_currentState == UIState.MainMenu && newState == UIState.HUD) {
				CleanupUIInstance(_currentState);
			}

			HideAllUI();
			_currentState = newState;

			if (newState != UIState.None) {
				ShowUI(newState);
			}

			HandlePauseState(newState);
			UpdateBackgroundVisibility();
		}

		private void HideAllUI() {
			foreach (var ui in _uiInstances.Values) {
				if (ui != null && IsInstanceValid(ui)) {
					ui.Visible = false;
				}
			}
		}

		private void ShowUI(UIState state) {
			// Only recreate if instance doesn't exist or is invalid
			if (!_uiInstances.TryGetValue(state, out Control value) || !IsInstanceValid(value)) {
				CreateUIInstance(state);
			}

			var ui = _uiInstances[state];
			if (ui != null) {
				ui.Visible = true;
				ui.ProcessMode = ProcessModeEnum.Always;
			}
		}

		private void CreateUIInstance(UIState state) {
			if (!_uiPaths.ContainsKey(state)) return;

			string path = _uiPaths[state];
			if (!ResourceLoader.Exists(path)) {
				GD.PrintErr($"UI resource not found: {path}");
				return;
			}

			var scene = GD.Load<PackedScene>(path);
			var instance = scene?.Instantiate<Control>();
			if (instance == null) return;

			instance.Name = state.ToString();
			instance.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
			instance.Visible = false;
			instance.ProcessMode = ProcessModeEnum.Always;
			
			// Ensure UI instances are above background elements
			instance.ZIndex = 0;

			AddChild(instance);
			_uiInstances[state] = instance;

			// Fix: Cast enum to int for CallDeferred
			CallDeferred(nameof(OnUIInstanceCreated), (int)state);
		}
		
		private void OnUIInstanceCreated(int stateInt) {
			UIState state = (UIState)stateInt;
			if (_uiInstances.TryGetValue(state, out Control instance) && instance != null) {
				// Force the _Ready method to be called if it hasn't been already
				if (!instance.IsInsideTree()) {
					GD.PrintErr($"UI instance {state} not properly added to tree");
					return;
				}
				
				// Call a method to ensure initialization if the UI supports it
				if (instance.HasMethod("EnsureInitialized")) {
					instance.Call("EnsureInitialized");
				}
			}
		}

		private void HandlePauseState(UIState newState) {
			switch (newState) {
				case UIState.PauseMenu:
					_isPaused = true;
					// Don't pause the tree, just disable player input
					GameEvents.EmitPlayerInputEnabled(false);
					Input.MouseMode = Input.MouseModeEnum.Visible;
					break;
				case UIState.HUD:
					if (_isPaused) {
						_isPaused = false;
						// Re-enable player input
						GameEvents.EmitPlayerInputEnabled(true);
						Input.MouseMode = Input.MouseModeEnum.Captured;
					}
					break;
				case UIState.MainMenu:
				case UIState.SettingsMenu:
					// Always disable player input when in menu states
					if (_isPaused) {
						_isPaused = false;
					}
					GameEvents.EmitPlayerInputEnabled(false);
					Input.MouseMode = Input.MouseModeEnum.Visible;
					break;
			}
		}

		public void PauseGame() {
			if (_currentState == UIState.HUD && !_isPaused) {
				// Use events instead of direct state change
				GameEvents.EmitGamePauseRequested();
			}
		}

		public void ResumeGame() {
			if (_currentState == UIState.PauseMenu && _isPaused) {
				// Use events instead of direct state change
				GameEvents.EmitGameResumeRequested();
			}
		}

		public bool IsPaused() => _isPaused;

		// Add cleanup method for when UI instances should be completely removed
		public void CleanupUIInstance(UIState state) {
			if (_uiInstances.TryGetValue(state, out Control instance) && instance != null) {
				if (IsInstanceValid(instance)) {
					instance.QueueFree();
				}
				_uiInstances.Remove(state);
			}
		}

		private void UpdateBackgroundVisibility() {
			if (_mainBG == null || _opacityBG == null) return;

			_mainBG.Visible = false;
			_opacityBG.Visible = false;

			switch (_currentState) {
				case UIState.MainMenu:
					_mainBG.Visible = true;
					break;
				case UIState.SettingsMenu:
					// Show opacity background if HUD exists, else show main background
					_mainBG.Visible = !(_uiInstances.ContainsKey(UIState.HUD) && _uiInstances[UIState.HUD] != null);
					_opacityBG.Visible = !_mainBG.Visible;
					break;
				case UIState.PauseMenu:
					_opacityBG.Visible = true;
					break;
			}
		}

		private void UpdateOpacityBackground() {
			if (_opacityBG == null || GameManager.Instance?.Settings == null) return;

			float opacity = GameManager.Instance.Settings.GetMenuOpacity();
			Color currentColor = _opacityBG.Color;
			_opacityBG.Color = new Color(currentColor.R, currentColor.G, currentColor.B, opacity);
		}

		private void OnSettingUpdated(string settingName, Variant value) {
			if (settingName == "ui_menu_opacity") {
				UpdateOpacityBackground();
			}
		}

		// Override _ExitTree to clean up properly but don't clear Instance
		public override void _ExitTree() {
			// Clean up UI instances only
			foreach (var kvp in _uiInstances) {
				if (kvp.Value != null && IsInstanceValid(kvp.Value)) {
					kvp.Value.QueueFree();
				}
			}
			_uiInstances.Clear();

			// Background elements will be cleaned up automatically as children
			// No need to manually clean them up since they're part of the UIManager scene tree

			// Unsubscribe from static events
			GameEvents.UIStateChanged -= OnUIStateChanged;
			GameEvents.SettingUpdated -= OnSettingUpdated;

			if (Instance == this) {
				Instance = null;
			}
		}
	}
}
