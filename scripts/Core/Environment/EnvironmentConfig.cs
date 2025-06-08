using Godot;
using System;
using System.Collections.Generic;
using System.IO;

namespace DangboxGame.Scripts.Core.Environment {
	public partial class EnvironmentConfig : Node {
		public static EnvironmentConfig Instance { get; private set; }
		
		public enum EnvironmentType {
			Development,
			Production,
			Testing
		}
		
		private EnvironmentType _currentEnvironment;
		private Dictionary<string, Variant> _config = new();
		private string _configPath;
		
		public override void _Ready() {
			if (Instance != null) {
				QueueFree();
				return;
			}
			Instance = this;
			ProcessMode = ProcessModeEnum.Always;
			
			DetectEnvironment();
			LoadConfiguration();
			EnsureDirectoriesExist();
			
			GD.Print($"EnvironmentConfig initialized for {_currentEnvironment} environment");
			GD.Print($"Player prefab path: {GetValue<string>("prefab_player")}");
			GD.Print($"Camera prefab path: {GetValue<string>("prefab_camera")}");
		}
		
		private void DetectEnvironment() {
			// Detect environment based on various factors
			if (OS.IsDebugBuild()) {
				_currentEnvironment = EnvironmentType.Development;
			} else if (OS.HasEnvironment("DANGBOX_TESTING")) {
				_currentEnvironment = EnvironmentType.Testing;
			} else {
				_currentEnvironment = EnvironmentType.Production;
			}
			
			// Override with command line argument if present
			var args = OS.GetCmdlineArgs();
			foreach (string arg in args) {
				if (arg.StartsWith("--env=")) {
					string envName = arg.Substring(6);
					if (Enum.TryParse<EnvironmentType>(envName, true, out EnvironmentType env)) {
						_currentEnvironment = env;
					}
				}
			}
		}
		
		private void LoadConfiguration() {
			// Determine config file path based on system and environment
			_configPath = GetSystemSpecificConfigPath();
			
			// Load base configuration
			LoadBaseConfiguration();
			
			// Load environment-specific overrides
			LoadEnvironmentConfiguration();
			
			// Ensure critical directories exist
			EnsureDirectoriesExist();
		}
		
		private string GetSystemSpecificConfigPath() {
			string baseDir;
			
			// System-specific base directory selection
			if (OS.GetName() == "Windows") {
				baseDir = OS.GetEnvironment("APPDATA") + "/DangboxGame/";
			} else if (OS.GetName() == "macOS") {
				baseDir = OS.GetEnvironment("HOME") + "/Library/Application Support/DangboxGame/";
			} else { // Linux and others
				baseDir = OS.GetEnvironment("HOME") + "/.local/share/dangbox-game/";
			}
			
			// Environment-specific subdirectory
			string envDir = _currentEnvironment.ToString().ToLower();
			return Path.Combine(baseDir, envDir, "config.json").Replace("\\", "/");
		}
		
		private void LoadBaseConfiguration() {
			// Base configuration - these are the defaults
			_config = new Dictionary<string, Variant> {
				// Scene Paths
				["scene_main_menu"] = "res://scenes/ui/MainScreen.tscn",
				["scene_settings_menu"] = "res://scenes/ui/SettingsMenu.tscn",
				["scene_pause_menu"] = "res://scenes/ui/PauseMenu.tscn",
				["scene_test_level"] = "res://scenes/TestLevel.tscn",
				["scene_main_game"] = "res://scenes/Main.tscn",
				
				// Prefab Paths
				["prefab_player"] = "res://assets/prefabs/character_body_3d.tscn",
				["prefab_camera"] = "res://assets/prefabs/_camera.tscn",
				["prefab_hud"] = "res://scenes/ui/HUD.tscn",
				
				// Script Paths
				["script_player_input"] = "res://scripts/player/PlayerInput.cs",
				
				// Data Paths (system-specific)
				["data_saves"] = GetSystemSpecificDataPath("saves"),
				["data_settings"] = GetSystemSpecificDataPath("settings"),
				["data_temp"] = GetSystemSpecificDataPath("temp"),
				["data_logs"] = GetSystemSpecificDataPath("logs"),
				
				// Environment-specific settings
				["debug_enabled"] = _currentEnvironment == EnvironmentType.Development,
				["logging_level"] = _currentEnvironment == EnvironmentType.Development ? "Debug" : "Info",
				["auto_save_interval"] = _currentEnvironment == EnvironmentType.Testing ? 10.0f : 60.0f
			};
		}
		
		private string GetSystemSpecificDataPath(string category) {
			string baseDir;
			
			if (OS.GetName() == "Windows") {
				baseDir = OS.GetEnvironment("USERPROFILE") + "/Documents/My Games/DangboxGame/";
			} else if (OS.GetName() == "macOS") {
				baseDir = OS.GetEnvironment("HOME") + "/Documents/DangboxGame/";
			} else { // Linux
				baseDir = OS.GetEnvironment("HOME") + "/.local/share/dangbox-game/";
			}
			
			string envDir = _currentEnvironment.ToString().ToLower();
			return Path.Combine(baseDir, envDir, category).Replace("\\", "/") + "/";
		}
		
		private void LoadEnvironmentConfiguration() {
			// Try to load existing config file
			if (File.Exists(_configPath)) {
				try {
					string jsonContent = File.ReadAllText(_configPath);
					var json = Json.ParseString(jsonContent);
					
					if (json.AsGodotDictionary() is Godot.Collections.Dictionary dict) {
						foreach (var kvp in dict) {
							_config[kvp.Key.AsString()] = kvp.Value;
						}
					}
					
					GD.Print($"Loaded environment config from: {_configPath}");
				} catch (Exception e) {
					GD.PrintErr($"Failed to load environment config: {e.Message}");
				}
			} else {
				// Create default config file
				SaveConfiguration();
			}
		}
		
		private void EnsureDirectoriesExist() {
			try {
				// Ensure config directory exists
				string configDir = Path.GetDirectoryName(_configPath);
				if (!Directory.Exists(configDir)) {
					Directory.CreateDirectory(configDir);
				}
				
				// Ensure data directories exist
				foreach (var key in _config.Keys) {
					if (key.StartsWith("data_")) {
						string path = _config[key].AsString();
						if (!Directory.Exists(path)) {
							Directory.CreateDirectory(path);
							GD.Print($"Created directory: {path}");
						}
					}
				}
			} catch (Exception e) {
				GD.PrintErr($"Failed to create directories: {e.Message}");
			}
		}
		
		public void SaveConfiguration() {
			try {
				var dict = new Godot.Collections.Dictionary();
				foreach (var kvp in _config) {
					dict[kvp.Key] = kvp.Value;
				}
				
				string jsonContent = Json.Stringify(dict);
				
				string configDir = Path.GetDirectoryName(_configPath);
				if (!Directory.Exists(configDir)) {
					Directory.CreateDirectory(configDir);
				}
				
				File.WriteAllText(_configPath, jsonContent);
				GD.Print($"Saved environment config to: {_configPath}");
			} catch (Exception e) {
				GD.PrintErr($"Failed to save environment config: {e.Message}");
			}
		}
		
		public T GetValue<[MustBeVariant] T>(string key, T defaultValue = default) {
			if (_config.TryGetValue(key, out Variant value)) {
				try {
					return value.As<T>();
				} catch {
					GD.PrintErr($"Failed to convert config value '{key}' to type {typeof(T)}");
				}
			}
			return defaultValue;
		}
		
		public void SetValue<[MustBeVariant] T>(string key, T value) {
			_config[key] = Variant.From(value);
		}
		
		public EnvironmentType GetCurrentEnvironment() => _currentEnvironment;
		
		public string GetConfigPath() => _configPath;
		
		public override void _ExitTree() {
			SaveConfiguration();
			if (Instance == this) {
				Instance = null;
			}
		}
	}
}
