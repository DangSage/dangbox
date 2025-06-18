using Godot;
using System;
using DangboxGame.Scripts.Player;

namespace DangboxGame.Scripts.UI {
	public partial class SettingsMenu : Control {
		// UI elements
		private Tree _settingsTree;
		private HSlider _fovSlider;
		private HSlider _resolutionScaleSlider;
		private HSlider _masterVolumeSlider;
		private Button _backButton;
		private Button _applyButton;
		private Label _fovValueLabel;
		
		// Tree items
		private TreeItem _rootItem;
		private TreeItem _graphicsItem;
		private TreeItem _fullscreenItem;
		private TreeItem _resScaleItem;
		private TreeItem _soundItem;
		private TreeItem _masterVolumeItem;
		private TreeItem _uiItem;
		private TreeItem _menuOpacityItem;
		private TreeItem _controlsItem;
		private TreeItem _sensitivityItem;
		
		// Resolution scale slider (created dynamically)
		private HSlider _resScaleSlider;
		
		// Keep track of changed settings
		private bool _settingsChanged = false;

		public override void _Ready() {
			// Get UI elements
			_settingsTree = GetNode<Tree>("VBoxContainer/SettingsTree");
			_fovSlider = GetNode<HSlider>("VBoxContainer/FOVContainer/FOVSlider");
			_fovValueLabel = GetNode<Label>("VBoxContainer/FOVContainer/ValueLabel");
			_backButton = GetNode<Button>("VBoxContainer/ButtonContainer/BackButton");
			_applyButton = GetNode<Button>("VBoxContainer/ButtonContainer/ApplyButton");
			
			// Subscribe to static events
			GameEvents.SettingUpdated += OnSettingUpdated;
			
			// Setup tree
			SetupSettingsTree();
			
			// Initialize values from GameSettings
			InitializeSettingsValues();
			
			// Connect signals
			ConnectSignals();
		}
		
		private void SetupSettingsTree() {
			// Configure tree
			_settingsTree.HideRoot = true;
			_settingsTree.Columns = 2;
			// _settingsTree.SetColumnTitle(0, "Setting");
			// _settingsTree.SetColumnTitle(1, "Value");
			_settingsTree.SetColumnExpand(0, true);
			_settingsTree.SetColumnExpand(1, true);
            _settingsTree.SetColumnCustomMinimumWidth(0, 150);
            _settingsTree.SetColumnExpand(1, true);
			
			// Create root item (hidden)
            _rootItem = _settingsTree.CreateItem();
			
			// Create Graphics section
			_graphicsItem = _settingsTree.CreateItem(_rootItem);
			_graphicsItem.SetText(0, "Graphics Settings");
			_graphicsItem.SetSelectable(0, false);
			_graphicsItem.Collapsed = false; // Start expanded
			
			// Fullscreen setting
			_fullscreenItem = _settingsTree.CreateItem(_graphicsItem);
			_fullscreenItem.SetText(0, "Fullscreen");
			_fullscreenItem.SetCellMode(1, TreeItem.TreeCellMode.Check);
			_fullscreenItem.SetEditable(1, true);
			
			// Resolution Scale setting
			_resScaleItem = _settingsTree.CreateItem(_graphicsItem);
			_resScaleItem.SetText(0, "Resolution Scale");
			_resScaleItem.SetText(1, "100%");
			_resScaleItem.SetSelectable(1, true);
			_resScaleItem.SetEditable(1, false);
			
			// Sound section
			_soundItem = _settingsTree.CreateItem(_rootItem);
			_soundItem.SetText(0, "Sound Settings");
			_soundItem.SetSelectable(0, false);
			_soundItem.Collapsed = false; // Start expanded
			
			// Master Volume setting
			_masterVolumeItem = _settingsTree.CreateItem(_soundItem);
			_masterVolumeItem.SetText(0, "Master Volume");
			_masterVolumeItem.SetText(1, "100%");
			_masterVolumeItem.SetSelectable(1, true);
			_masterVolumeItem.SetEditable(1, false);

			// UI section
			_uiItem = _settingsTree.CreateItem(_rootItem);
			_uiItem.SetText(0, "Interface Settings");
			_uiItem.SetSelectable(0, false);
			_uiItem.Collapsed = false; // Start expanded

			// Menu Opacity setting
			_menuOpacityItem = _settingsTree.CreateItem(_uiItem);
			_menuOpacityItem.SetText(0, "Menu Opacity");
			_menuOpacityItem.SetText(1, "80%");
			_menuOpacityItem.SetSelectable(1, true);
			_menuOpacityItem.SetEditable(1, false);

			// Controls section
			_controlsItem = _settingsTree.CreateItem(_rootItem);
			_controlsItem.SetText(0, "Controls");
			_controlsItem.SetSelectable(0, false);
			_controlsItem.Collapsed = false;

			// Mouse Sensitivity setting
			_sensitivityItem = _settingsTree.CreateItem(_controlsItem);
			_sensitivityItem.SetText(0, "Mouse Sensitivity");
			_sensitivityItem.SetText(1, "33%");
			_sensitivityItem.SetSelectable(1, true);
			_sensitivityItem.SetEditable(1, false);
		}
		
		private void InitializeSettingsValues() {
			// Access GameSettings through GameManager
			var settings = GameManager.Instance?.Settings;
			if (settings == null) {
				GD.PrintErr("GameSettings not available in SettingsMenu.InitializeSettingsValues");
				return;
			}
			
			// Field of View
			float fov = settings._settings.ContainsKey("graphics_fov") ? 
				(float)settings._settings["graphics_fov"] : 90.0f;
			_fovSlider.Value = fov;
			UpdateFOVLabel(fov);
			
			// Fullscreen
			bool fullscreen = settings.GetFullscreen();
			_fullscreenItem.SetChecked(1, fullscreen);
			
			// Resolution scale (convert to percentage for UI)
			float resScale = settings.GetResolutionScale();
			_resScaleItem.SetText(1, $"{resScale * 100:F0}%");
			
			// Master volume
			float volume = settings.GetMasterVolume();
			_masterVolumeItem.SetText(1, $"{volume * 100:F0}%");

			// Menu opacity
			float menuOpacity = settings.GetMenuOpacity();
			_menuOpacityItem.SetText(1, $"{menuOpacity * 100:F0}%");

			// Mouse sensitivity
			float sensitivity = settings.GetSensitivity();
			_sensitivityItem.SetText(1, $"{sensitivity * 100:F0}%");
		}
		
		private void ConnectSignals() {
			// Use SafeConnect instead of Connect to avoid errors if signals were already connected
			SafeConnect(_fovSlider, "value_changed", Callable.From<double>(OnFOVChanged));
			SafeConnect(_backButton, "pressed", Callable.From(OnBackPressed));
			SafeConnect(_applyButton, "pressed", Callable.From(OnApplyPressed));
			SafeConnect(_settingsTree, "item_selected", Callable.From(OnTreeItemSelected));
			SafeConnect(_settingsTree, "item_edited", Callable.From(OnTreeItemEdited));
			SafeConnect(_settingsTree, "item_activated", Callable.From(OnTreeItemActivated));
			
			// Make sure FOV is initialized
			if (_fovSlider != null && GameManager.Instance?.Settings != null) {
				_fovSlider.Value = GameManager.Instance.Settings.GetFOV();
				UpdateFOVLabel((float)_fovSlider.Value);
			}
		}
		
		// Helper method to safely connect signals
		private void SafeConnect(Node source, string signalName, Callable callable) {
			if (source == null) return;
			
			// Disconnect first if already connected to prevent duplicate signals
			var connections = source.GetSignalConnectionList(signalName);
			foreach (var conn in connections) {
				if (conn["callable"].As<Callable>().Target == callable.Target && 
					conn["callable"].As<Callable>().Method == callable.Method) {
					source.Disconnect(signalName, callable);
				}
			}
			
			// Now connect
			source.Connect(signalName, callable);
		}
		
		private void OnTreeItemSelected() {
			// When an item is selected, we might need to show controls
			var selected = _settingsTree.GetSelected();
			
			if (selected == _resScaleItem) {
				ShowResolutionScaleSlider();
			} else if (selected == _masterVolumeItem) {
				ShowVolumeSlider();
			} else if (selected == _menuOpacityItem) {
				ShowOpacitySlider();
			} else if (selected == _sensitivityItem) {
				ShowSensitivitySlider();
			} else {
				// Hide all sliders except FOV which is always visible
				RemoveTemporarySliders();
			}
		}
		
		private void ShowResolutionScaleSlider() {
			RemoveTemporarySliders();
			
			// Create resolution scale slider
			_resScaleSlider = new HSlider();
			_resScaleSlider.Name = "TempResScaleSlider";
			_resScaleSlider.MinValue = 50;
			_resScaleSlider.MaxValue = 200;
			_resScaleSlider.CustomMinimumSize = new Vector2(200, 0);
			_resScaleSlider.SizeFlagsHorizontal = SizeFlags.ExpandFill;
			_resScaleSlider.Step = 5;
			_resScaleSlider.TickCount = 7;
			_resScaleSlider.TicksOnBorders = true;
			
			// Get the current value from GameSettings
			float currentScale = 100;
			if (GameManager.Instance?.Settings != null) {
				currentScale = GameManager.Instance.Settings.GetResolutionScale() * 100;
			}
			_resScaleSlider.Value = currentScale;
			
			// Add it after the settings tree
			var container = new HBoxContainer();
			container.Name = "TempSliderContainer";
			container.SizeFlagsHorizontal = SizeFlags.ExpandFill;
			
			var label = new Label();
			label.Text = "Resolution Scale:";
			container.AddChild(label);
			
			container.AddChild(_resScaleSlider);
			
			var valueLabel = new Label();
			valueLabel.Name = "ResScaleValueLabel";
			valueLabel.Text = $"{_resScaleSlider.Value:F0}%";
			valueLabel.HorizontalAlignment = HorizontalAlignment.Right;
			container.AddChild(valueLabel);
			
			GetNode<VBoxContainer>("VBoxContainer").AddChild(container);
			GetNode<VBoxContainer>("VBoxContainer").MoveChild(container, 2);
			
			// Make sure to connect signal AFTER setting the initial value
			_resScaleSlider.Connect("value_changed", Callable.From<double>(OnResScaleChanged));
			
			// Manually update the label
			var valueLabelNode = container.GetNode<Label>("ResScaleValueLabel");
			if (valueLabelNode != null) {
				valueLabelNode.Text = $"{currentScale:F0}%";
			}
		}
		
		private void ShowVolumeSlider() {
			RemoveTemporarySliders();
			
			// Create master volume slider
			_masterVolumeSlider = new HSlider();
			_masterVolumeSlider.Name = "TempVolumeSlider";
			_masterVolumeSlider.MinValue = 0;
			_masterVolumeSlider.MaxValue = 100;
			_masterVolumeSlider.CustomMinimumSize = new Vector2(200, 0);
			_masterVolumeSlider.SizeFlagsHorizontal = SizeFlags.ExpandFill;
			_masterVolumeSlider.Step = 1;
			_masterVolumeSlider.TickCount = 5;
			_masterVolumeSlider.TicksOnBorders = true;
			
			// Get the current value from GameSettings
			float currentVolume = 100;
			if (GameManager.Instance?.Settings != null) {
				currentVolume = GameManager.Instance.Settings.GetMasterVolume() * 100;
			}
			_masterVolumeSlider.Value = currentVolume;
			
			// Add it after the settings tree
			var container = new HBoxContainer();
			container.Name = "TempSliderContainer";
			container.SizeFlagsHorizontal = SizeFlags.ExpandFill;
			
			var label = new Label();
			label.Text = "Volume:";
			container.AddChild(label);
			
			container.AddChild(_masterVolumeSlider);
			
			var valueLabel = new Label();
			valueLabel.Name = "VolumeValueLabel";
			valueLabel.Text = $"{_masterVolumeSlider.Value:F0}%";
			valueLabel.HorizontalAlignment = HorizontalAlignment.Right;
			container.AddChild(valueLabel);
			
			GetNode<VBoxContainer>("VBoxContainer").AddChild(container);
			
			// Insert it at the right position (after tree, before FOV)
			GetNode<VBoxContainer>("VBoxContainer").MoveChild(container, 2);
			
			// Connect value changed signal
			_masterVolumeSlider.Connect("value_changed", Callable.From<double>(OnVolumeChanged));
			
			// Manually update the label
			var valueLabelNode = container.GetNode<Label>("VolumeValueLabel");
			if (valueLabelNode != null) {
				valueLabelNode.Text = $"{currentVolume:F0}%";
			}
		}
		
		private void ShowOpacitySlider() {
			RemoveTemporarySliders();
			
			// Create menu opacity slider
			var opacitySlider = new HSlider();
			opacitySlider.Name = "TempOpacitySlider";
			opacitySlider.MinValue = 0;
			opacitySlider.MaxValue = 100;
			opacitySlider.CustomMinimumSize = new Vector2(200, 0);
			opacitySlider.SizeFlagsHorizontal = SizeFlags.ExpandFill;
			opacitySlider.Step = 5;
			opacitySlider.TickCount = 5;
			opacitySlider.TicksOnBorders = true;
			
			// Get the current value from GameSettings
			float currentOpacity = 80;
			if (GameManager.Instance?.Settings != null) {
				currentOpacity = GameManager.Instance.Settings.GetMenuOpacity() * 100;
			}
			opacitySlider.Value = currentOpacity;
			
			// Add it after the settings tree
			var container = new HBoxContainer();
			container.Name = "TempSliderContainer";
			container.SizeFlagsHorizontal = SizeFlags.ExpandFill;
			
			var label = new Label();
			label.Text = "Menu Opacity:";
			container.AddChild(label);
			
			container.AddChild(opacitySlider);
			
			var valueLabel = new Label();
			valueLabel.Name = "OpacityValueLabel";
			valueLabel.Text = $"{opacitySlider.Value:F0}%";
			valueLabel.HorizontalAlignment = HorizontalAlignment.Right;
			container.AddChild(valueLabel);
			
			GetNode<VBoxContainer>("VBoxContainer").AddChild(container);
			GetNode<VBoxContainer>("VBoxContainer").MoveChild(container, 2);
			
			// Connect value changed signal
			opacitySlider.Connect("value_changed", Callable.From<double>(OnOpacityChanged));
			
			// Manually update the label
			var valueLabelNode = container.GetNode<Label>("OpacityValueLabel");
			if (valueLabelNode != null) {
				valueLabelNode.Text = $"{currentOpacity:F0}%";
			}
		}
		
		private void ShowSensitivitySlider() {
			RemoveTemporarySliders();
			
			var sensitivitySlider = new HSlider();
			sensitivitySlider.Name = "TempSensitivitySlider";
			sensitivitySlider.MinValue = 1;
			sensitivitySlider.MaxValue = 100;
			sensitivitySlider.CustomMinimumSize = new Vector2(200, 0);
			sensitivitySlider.SizeFlagsHorizontal = SizeFlags.ExpandFill;
			sensitivitySlider.Step = 1;
			sensitivitySlider.TickCount = 5;
			sensitivitySlider.TicksOnBorders = true;

			// Get current value
			float currentSensitivity = 33;  // Changed default fallback from 33 to match 0.33f * 100
			if (GameManager.Instance?.Settings != null) {
				currentSensitivity = GameManager.Instance.Settings.GetSensitivity() * 100;
			}
			sensitivitySlider.Value = currentSensitivity;
			
			var container = new HBoxContainer();
			container.Name = "TempSliderContainer";
			container.SizeFlagsHorizontal = SizeFlags.ExpandFill;
			
			var label = new Label();
			label.Text = "Mouse Sensitivity:";
			container.AddChild(label);
			
			container.AddChild(sensitivitySlider);
			
			var valueLabel = new Label();
			valueLabel.Name = "SensitivityValueLabel";
			valueLabel.Text = $"{sensitivitySlider.Value:F0}%";
			valueLabel.HorizontalAlignment = HorizontalAlignment.Right;
			container.AddChild(valueLabel);
			
			GetNode<VBoxContainer>("VBoxContainer").AddChild(container);
			GetNode<VBoxContainer>("VBoxContainer").MoveChild(container, 2);
			
			sensitivitySlider.Connect("value_changed", Callable.From<double>(OnSensitivityChanged));
		}
		
		private void RemoveTemporarySliders() {
			var tempContainer = GetNodeOrNull<Node>("VBoxContainer/TempSliderContainer");
			tempContainer?.QueueFree();
		}
		
		private void OnTreeItemEdited() {
			var edited = _settingsTree.GetEdited();
			
			if (edited == _fullscreenItem) {
				bool isChecked = _fullscreenItem.IsChecked(1);
				if (GameManager.Instance?.Settings != null) {
					GameManager.Instance.Settings.SetFullscreen(isChecked);
					_settingsChanged = true;
				}
			}
		}
		
		private void OnTreeItemActivated() {
			var activated = _settingsTree.GetSelected();
			
			// Toggle section expansion
			if (activated == _graphicsItem || activated == _soundItem || activated == _uiItem) {
				activated.Collapsed = !activated.Collapsed;
			}
		}
		
		private void OnResScaleChanged(double value) {
			float resScale = (float)value / 100.0f; // Convert percentage to scale factor
			
			if (GameManager.Instance?.Settings != null) {
				GameManager.Instance.Settings.SetResolutionScale(resScale);
				_settingsChanged = true;

				_resScaleItem.SetText(1, $"{value:F0}%");
				
				// Update slider value label if it exists
				var valueLabel = GetNodeOrNull<Label>("VBoxContainer/TempSliderContainer/ResScaleValueLabel");
				if (valueLabel != null) {
					valueLabel.Text = $"{value:F0}%";
				}
			}
		}
		
		private void OnFOVChanged(double value) {
			float fov = (float)value;
			if (GameManager.Instance?.Settings != null) {
				GameManager.Instance.Settings.SetFOV(fov);
				_settingsChanged = true;
				
				// Update label
				UpdateFOVLabel(fov);
				
				// Apply FOV change immediately to camera if it exists
				ApplyFOVToCamera(fov);
			}
		}
		
		private void UpdateFOVLabel(float value) {
			if (_fovValueLabel != null) {
				_fovValueLabel.Text = $"{value:F0}°";
			}
		}
		
		private void OnVolumeChanged(double value) {
			float volume = (float)value / 100.0f; // Convert to 0-1 range
			if (GameManager.Instance?.Settings != null) {
				GameManager.Instance.Settings.SetMasterVolume(volume);
				_settingsChanged = true;
				
				// Update tree item and value label
				_masterVolumeItem.SetText(1, $"{value:F0}%");
				
				// Update slider value label if it exists
				var valueLabel = GetNodeOrNull<Label>("VBoxContainer/TempSliderContainer/VolumeValueLabel");
				if (valueLabel != null) {
					valueLabel.Text = $"{value:F0}%";
				}
			}
		}
		
		private void OnOpacityChanged(double value) {
			float opacity = (float)value / 100.0f; // Convert to 0-1 range
			if (GameManager.Instance?.Settings != null) {
				GameManager.Instance.Settings.SetMenuOpacity(opacity);
				_settingsChanged = true;
				
				// Update tree item and value label
				_menuOpacityItem.SetText(1, $"{value:F0}%");
				
				// Update slider value label if it exists
				var valueLabel = GetNodeOrNull<Label>("VBoxContainer/TempSliderContainer/OpacityValueLabel");
				if (valueLabel != null) {
					valueLabel.Text = $"{value:F0}%";
				}
			}
		}
		
		private void OnSensitivityChanged(double value) {
			float sensitivity = (float)value / 100.0f;
			if (GameManager.Instance?.Settings != null) {
				GameManager.Instance.Settings.SetSensitivity(sensitivity);
				_settingsChanged = true;
				
				_sensitivityItem.SetText(1, $"{value:F0}%");
				
				var valueLabel = GetNodeOrNull<Label>("VBoxContainer/TempSliderContainer/SensitivityValueLabel");
				if (valueLabel != null) {
					valueLabel.Text = $"{value:F0}%";
				}
			}
		}
		
		private void ApplyFOVToCamera(float fov) {
			// Find the main camera in the scene and apply FOV
			var camera = GetViewport().GetCamera3D();
			if (camera != null) {
				camera.Fov = fov;
			}
		}
		
		private void OnBackPressed() {
			// Save changes if any were made
			if (_settingsChanged && GameManager.Instance?.Settings != null) {
				GameManager.Instance.Settings.SaveSettings();
			}

			var playerManager = GameManager.Instance?.PlayerManager;
			var localPlayer = playerManager?.GetLocalPlayer();

			if (localPlayer is PlayerController player) {
				// If the player is in-game, return to the base pause menu
				UIManager.Instance.ChangeUIState(UIManager.UIState.PauseMenu);
			} else {
				// Otherwise, go to the main menu
				UIManager.Instance.ChangeUIState(UIManager.UIState.MainMenu);
			}
		}
		
		private void OnApplyPressed() {
			if (GameManager.Instance?.Settings != null) {
				GameManager.Instance.Settings.SaveSettings();
				GameManager.Instance.Settings.EmitSignal(GameSettings.SignalName.SettingsUpdated);
				_settingsChanged = false;
				
				RefreshAllValues();
				
				var notification = new Label();
				notification.Text = "Settings Applied";
				notification.HorizontalAlignment = HorizontalAlignment.Center;
				notification.AddThemeColorOverride("font_color", Colors.LightGreen);
				
				GetNode<VBoxContainer>("VBoxContainer").AddChild(notification);
				
				var timer = GetTree().CreateTimer(2.0);
				timer.Timeout += () => {
					if (notification != null && !notification.IsQueuedForDeletion()) {
						notification.QueueFree();
					}
				};
			}
		}
		
		// Add a method to refresh all visible values from GameSettings
		private void RefreshAllValues() {
			if (GameManager.Instance?.Settings == null) return;
			
			var settings = GameManager.Instance.Settings;
			
			// Update tree items
			if (_fullscreenItem != null) {
				_fullscreenItem.SetChecked(1, settings.GetFullscreen());
			}
			
			if (_resScaleItem != null) {
				float resScale = settings.GetResolutionScale();
				_resScaleItem.SetText(1, $"{resScale * 100:F0}%");
			}
			
			if (_masterVolumeItem != null) {
				float volume = settings.GetMasterVolume();
				_masterVolumeItem.SetText(1, $"{volume * 100:F0}%");
			}

			if (_menuOpacityItem != null) {
				float opacity = settings.GetMenuOpacity();
				_menuOpacityItem.SetText(1, $"{opacity * 100:F0}%");
			}
			
			if (_sensitivityItem != null) {
				float sensitivity = settings.GetSensitivity();
				_sensitivityItem.SetText(1, $"{sensitivity * 100:F0}%");
			}
			
			// Update FOV slider
			if (_fovSlider != null) {
				// _fovSlider.Value = settings.GetFOV();
                _fovSlider.SetValueNoSignal(settings.GetFOV());
				UpdateFOVLabel((float)_fovSlider.Value);
			}
			
			// Update any visible dynamic sliders
			var resScaleValueLabel = GetNodeOrNull<Label>("VBoxContainer/TempSliderContainer/ResScaleValueLabel");
			if (resScaleValueLabel != null && _resScaleSlider != null) {
				_resScaleSlider.Value = settings.GetResolutionScale() * 100;
				resScaleValueLabel.Text = $"{_resScaleSlider.Value:F0}%";
			}
			
			var volumeValueLabel = GetNodeOrNull<Label>("VBoxContainer/TempSliderContainer/VolumeValueLabel");
			if (volumeValueLabel != null && _masterVolumeSlider != null) {
				_masterVolumeSlider.Value = settings.GetMasterVolume() * 100;
				volumeValueLabel.Text = $"{_masterVolumeSlider.Value:F0}%";
			}

			var opacityValueLabel = GetNodeOrNull<Label>("VBoxContainer/TempSliderContainer/OpacityValueLabel");
			if (opacityValueLabel != null) {
				var opacitySlider = GetNodeOrNull<HSlider>("VBoxContainer/TempSliderContainer/TempOpacitySlider");
				if (opacitySlider != null) {
					opacitySlider.Value = settings.GetMenuOpacity() * 100;
					opacityValueLabel.Text = $"{opacitySlider.Value:F0}%";
				}
			}

			var sensitivityValueLabel = GetNodeOrNull<Label>("VBoxContainer/TempSliderContainer/SensitivityValueLabel");
			if (sensitivityValueLabel != null) {
				var sensitivitySlider = GetNodeOrNull<HSlider>("VBoxContainer/TempSliderContainer/TempSensitivitySlider");
				if (sensitivitySlider != null) {
					sensitivitySlider.Value = settings.GetSensitivity() * 100;
					sensitivityValueLabel.Text = $"{sensitivitySlider.Value:F0}%";
				}
			}
		}
		
		// Override _Process to handle dynamic UI updates
		public override void _Process(double delta) {
			base._Process(delta);
			
			// Update temporary slider labels in real-time if they exist
			if (_resScaleSlider != null) {
				var valueLabel = GetNodeOrNull<Label>("VBoxContainer/TempSliderContainer/ResScaleValueLabel");
				if (valueLabel != null) {
					valueLabel.Text = $"{_resScaleSlider.Value:F0}%";
				}
			}
			
			if (_masterVolumeSlider != null) {
				var valueLabel = GetNodeOrNull<Label>("VBoxContainer/TempSliderContainer/VolumeValueLabel");
				if (valueLabel != null) {
					valueLabel.Text = $"{_masterVolumeSlider.Value:F0}%";
				}
			}

			// Fix: Check for opacity slider instead of _resScaleSlider
			var opacitySlider = GetNodeOrNull<HSlider>("VBoxContainer/TempSliderContainer/TempOpacitySlider");
			if (opacitySlider != null) {
				var valueLabel = GetNodeOrNull<Label>("VBoxContainer/TempSliderContainer/OpacityValueLabel");
				if (valueLabel != null) {
					valueLabel.Text = $"{opacitySlider.Value:F0}%";
				}
			}

			var sensitivitySlider = GetNodeOrNull<HSlider>("VBoxContainer/TempSliderContainer/TempSensitivitySlider");
			if (sensitivitySlider != null) {
				var valueLabel = GetNodeOrNull<Label>("VBoxContainer/TempSliderContainer/SensitivityValueLabel");
				if (valueLabel != null) {
					valueLabel.Text = $"{sensitivitySlider.Value:F0}%";
				}
			}
		}

		private void OnSettingUpdated(string settingName, Variant value) {
			// Update UI when settings change from other sources
			switch (settingName) {
				case "ui_menu_opacity":
					if (_menuOpacityItem != null) {
						float opacity = value.AsSingle();
						_menuOpacityItem.SetText(1, $"{opacity * 100:F0}%");
						
						// Update slider if visible
						var opacitySlider = GetNodeOrNull<HSlider>("VBoxContainer/TempSliderContainer/TempOpacitySlider");
						if (opacitySlider != null) {
							opacitySlider.Value = opacity * 100;
						}
					}
					break;
				case "volume_master":
					if (_masterVolumeItem != null) {
						float volume = value.AsSingle();
						_masterVolumeItem.SetText(1, $"{volume * 100:F0}%");
						
						// Update slider if visible
						var volumeSlider = GetNodeOrNull<HSlider>("VBoxContainer/TempSliderContainer/TempVolumeSlider");
						if (volumeSlider != null) {
							volumeSlider.Value = volume * 100;
						}
					}
					break;
				case "resolution_scale":
					if (_resScaleItem != null) {
						float scale = value.AsSingle();
						_resScaleItem.SetText(1, $"{scale * 100:F0}%");
						
						// Update slider if visible
						if (_resScaleSlider != null) {
							_resScaleSlider.Value = scale * 100;
						}
					}
					break;
				case "graphics_fov":
					if (_fovSlider != null) {
						float fov = value.AsSingle();
						_fovSlider.Value = fov;
						UpdateFOVLabel(fov);
					}
					break;
				case "fullscreen":
					if (_fullscreenItem != null) {
						bool fullscreen = value.AsBool();
						_fullscreenItem.SetChecked(1, fullscreen);
					}
					break;
				case "input_sensitivity":
					if (_sensitivityItem != null) {
						float sensitivity = value.AsSingle();
						_sensitivityItem.SetText(1, $"{sensitivity * 100:F0}%");
						
						var sensitivitySlider = GetNodeOrNull<HSlider>("VBoxContainer/TempSliderContainer/TempSensitivitySlider");
						if (sensitivitySlider != null) {
							sensitivitySlider.Value = sensitivity * 100;
						}
					}
					break;
			}
		}

		public override void _ExitTree() {
			// Unsubscribe from static events
			GameEvents.SettingUpdated -= OnSettingUpdated;
			base._ExitTree();
		}
	}
}
