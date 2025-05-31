using Godot;
using System;
using System.Collections.Generic;

namespace Dangbox {
	public partial class GameSettings : Node {
		private const string DefaultSavePath = "user://saves/";
		private const string SettingsFilePath = "user://game_settings.cfg";

		private readonly Dictionary<string, Variant> _settings = new() {
			{ "save_path", DefaultSavePath }
		};

		[Signal]
		public delegate void SettingsUpdatedEventHandler();

		public override void _Ready() {
			LoadSettings();
		}

		public string GetSavePath() {
			return _settings.ContainsKey("save_path") ? _settings["save_path"].AsString() : DefaultSavePath;
		}

		public void SetSavePath(string newPath) {
			_settings["save_path"] = newPath;
			EmitSignal(nameof(SettingsUpdatedEventHandler));
			SaveSettings();
		}

		private void LoadSettings() {
			var configFile = new ConfigFile();
			var error = configFile.Load(SettingsFilePath);

			if (error == Error.Ok) {
				foreach (var key in _settings.Keys) {
					if (configFile.HasSectionKey("settings", key)) {
						_settings[key] = configFile.GetValue("settings", key, _settings[key]);
					}
				}
			} else {
				GD.PrintErr($"Failed to load settings from {SettingsFilePath}. Using defaults.");
			}
		}

		private void SaveSettings() {
			var configFile = new ConfigFile();

			foreach (var key in _settings.Keys) {
				configFile.SetValue("settings", key, _settings[key]);
			}

			var error = configFile.Save(SettingsFilePath);
			if (error != Error.Ok) {
				GD.PrintErr($"Failed to save settings to {SettingsFilePath}.");
			}
		}
	}
}
