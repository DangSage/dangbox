using Godot;
using System;
using System.Collections.Generic;

namespace DangboxGame.Scripts {
	public partial class GameSettings : Node {
		private string SettingsFilePath => System.IO.Path.Combine(Constants.DataPath.Settings, "game_settings.cfg");

		// Make settings accessible for UI
		public readonly Dictionary<string, Variant> _settings = new() {
			{ "save_path", Constants.DataPath.Saves },
			{ "resolution_width", 1920 },
			{ "resolution_height", 1080 },
			{ "resolution_scale", 1.0f },
			{ "fullscreen", true },
			{ "volume_master", 1.0f },
			{ "volume_music", 0.8f },
			{ "volume_sfx", 1.0f },
			{ "ui_menu_opacity", 0.8f },  // New setting for menu backgrounds
			{ "graphics_fov", 90.0f },  // Default FOV
			{ "input_sensitivity", 0.33f }  // Changed from 0.1f to 0.33f
		};

		[Signal]
		public delegate void SettingsUpdatedEventHandler();

		public override void _Ready() {
			// Make the node persistent
			ProcessMode = ProcessModeEnum.Always;

			LoadSettings();
			ApplyInitialSettings();
		}

		private void ApplyInitialSettings() {
			// Apply fullscreen setting
			bool fullscreen = _settings.ContainsKey("fullscreen") ?
				(bool)_settings["fullscreen"] : true;
			DisplayServer.WindowSetMode(fullscreen ?
				DisplayServer.WindowMode.Fullscreen :
				DisplayServer.WindowMode.Windowed);

			// Apply master volume
			float volume = _settings.ContainsKey("volume_master") ?
				(float)_settings["volume_master"] : 1.0f;
			AudioServer.SetBusVolumeDb(
				AudioServer.GetBusIndex("Master"),
				volume <= 0 ? -80 : Mathf.LinearToDb(volume));
		}

		public string GetSavePath() {
			return _settings.ContainsKey("save_path") ? _settings["save_path"].AsString() : Constants.DataPath.Saves;
		}

		public void SetSavePath(string newPath) {
			_settings["save_path"] = newPath;
			GameEvents.EmitSettingUpdated("save_path", newPath);
			SaveSettings();
		}

		public float GetResolutionScale() {
			return _settings.ContainsKey("resolution_scale") ?
				(float)_settings["resolution_scale"] : 1.0f;
		}

		public void SetResolutionScale(float scale) {
			if (scale < 0.1f || scale > 4.0f) return;
			_settings["resolution_scale"] = scale;

			GetViewport().Scaling3DScale = scale;

			GameEvents.EmitSettingUpdated("resolution_scale", scale);
		}

		public bool GetFullscreen() {
			return _settings.ContainsKey("fullscreen") ?
				(bool)_settings["fullscreen"] : true;
		}

		public void SetFullscreen(bool value) {
			_settings["fullscreen"] = value;
			DisplayServer.WindowSetMode(value ?
				DisplayServer.WindowMode.Fullscreen :
				DisplayServer.WindowMode.Windowed);
			GameEvents.EmitSettingUpdated("fullscreen", value);
		}

		public float GetMasterVolume() {
			return _settings.ContainsKey("volume_master") ?
				(float)_settings["volume_master"] : 1.0f;
		}

		public void SetMasterVolume(float value) {
			value = Mathf.Clamp(value, 0f, 1f);
			_settings["volume_master"] = value;
			AudioServer.SetBusVolumeDb(
				AudioServer.GetBusIndex("Master"),
				value <= 0 ? -80 : Mathf.LinearToDb(value));
			GameEvents.EmitSettingUpdated("volume_master", value);
		}

		public float GetFOV() {
			return _settings.ContainsKey("graphics_fov") ?
				(float)_settings["graphics_fov"] : 90.0f;
		}

		public void SetFOV(float value) {
			value = Mathf.Clamp(value, 60f, 120f);
			_settings["graphics_fov"] = value;

			// Apply to current camera if it exists
			var viewport = GetViewport();
			if (viewport != null) {
				var camera = viewport.GetCamera3D();
				if (camera != null) {
					camera.Fov = value;
				}
			}

			GameEvents.EmitSettingUpdated("graphics_fov", value);
		}

		public float GetMenuOpacity() {
			return _settings.ContainsKey("ui_menu_opacity") ?
				(float)_settings["ui_menu_opacity"] : 0.8f;
		}

		public void SetMenuOpacity(float value) {
			value = Mathf.Clamp(value, 0f, 1f);
			_settings["ui_menu_opacity"] = value;
			GameEvents.EmitSettingUpdated("ui_menu_opacity", value);
		}

		public float GetSensitivity() {
			return _settings.ContainsKey("input_sensitivity") ?
				(float)_settings["input_sensitivity"] : 0.33f;  // Changed fallback from 0.1f to 0.33f
		}

		public void SetSensitivity(float value) {
			value = Mathf.Clamp(value, 0.01f, 1.0f);
			_settings["input_sensitivity"] = value;
			GameEvents.EmitSettingUpdated("input_sensitivity", value);
		}

		public void LoadSettings() {
			var configFile = new ConfigFile();
			var error = configFile.Load(SettingsFilePath);

			if (error == Error.Ok) {
				// Efficiently load settings from file
				foreach (var key in _settings.Keys) {
					if (configFile.HasSectionKey("settings", key)) {
						_settings[key] = configFile.GetValue("settings", key);
					}
				}
			} else {
				//GD.PrintErr($"Failed to load settings from {SettingsFilePath}. Using defaults.");
				SaveSettings(); // Create default settings file
			}
		}

		public void SaveSettings() {
			var configFile = new ConfigFile();

			// Store all settings in config file
			foreach (var entry in _settings) {
				configFile.SetValue("settings", entry.Key, entry.Value);
			}

			configFile.Save(SettingsFilePath);
			GameEvents.EmitSettingsChanged();
		}

		public override void _ExitTree() {
			// Save settings when exiting the game
			SaveSettings();
			GD.Print("GameSettings saved and exiting.");
		}
	}
}
