using Godot;
using DangboxGame.Scripts.Core.Environment;

namespace DangboxGame.Scripts {
	public static class Constants {
		// Scene Paths
		public static class ScenePath {
			public static string MainMenu => EnvironmentConfig.Instance?.GetValue<string>("scene_main_menu") ?? "res://scenes/ui/MainScreen.tscn";
			public static string SettingsMenu => EnvironmentConfig.Instance?.GetValue<string>("scene_settings_menu") ?? "res://scenes/ui/SettingsMenu.tscn";
			public static string PauseMenu => EnvironmentConfig.Instance?.GetValue<string>("scene_pause_menu") ?? "res://scenes/ui/PauseMenu.tscn";
			public static string TestLevel => EnvironmentConfig.Instance?.GetValue<string>("scene_test_level") ?? "res://scenes/TestLevel.tscn";
			public static string MainGameScene => EnvironmentConfig.Instance?.GetValue<string>("scene_main_game") ?? "res://scenes/Main.tscn";
		}

		// Prefab Paths
		public static class PrefabPath {
			public static string Player => EnvironmentConfig.Instance?.GetValue<string>("prefab_player") ?? "res://assets/prefabs/character_body_3d.tscn";
			public static string Camera => EnvironmentConfig.Instance?.GetValue<string>("prefab_camera") ?? "res://assets/prefabs/_camera.tscn";
			public static string HUD => EnvironmentConfig.Instance?.GetValue<string>("prefab_hud") ?? "res://scenes/ui/HUD.tscn";
		}

		// Script Paths
		public static class ScriptPath {
			public static string PlayerInput => EnvironmentConfig.Instance?.GetValue<string>("script_player_input") ?? "res://scripts/player/PlayerInput.cs";
		}
		
		// Data Paths (new - system-specific)
		public static class DataPath {
			public static string Saves => EnvironmentConfig.Instance?.GetValue<string>("data_saves") ?? "user://saves/";
			public static string Settings => EnvironmentConfig.Instance?.GetValue<string>("data_settings") ?? "user://settings/";
			public static string Temp => EnvironmentConfig.Instance?.GetValue<string>("data_temp") ?? "user://temp/";
			public static string Logs => EnvironmentConfig.Instance?.GetValue<string>("data_logs") ?? "user://logs/";
		}
	}
}
