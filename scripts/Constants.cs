using Godot;

namespace DangboxGame.Scripts {
	public static class Constants {
		// Scene Paths
		public static class ScenePath {
			public const string Home = "res://scenes/Home.tscn";
			public const string MainMenu = "res://scenes/ui/MainScreen.tscn";
			public const string TestLevel = "res://assets/prefabs/playtest_level.tscn";
			public const string SettingsMenu = "res://scenes/ui/SettingsMenu.tscn";
			public const string PauseMenu = "res://scenes/ui/PauseMenu.tscn";
		}

		// Prefab Paths
		public static class PrefabPath {
			public const string Player = "res://assets/prefabs/character_body_3d.tscn";
			public const string Camera = "res://assets/prefabs/_camera.tscn";
			public const string HUD = "res://scenes/ui/HUD.tscn";
		}

		// Script Paths
		public static class ScriptPath {
			public const string PlayerInput = "res://scripts/player/PlayerInput.cs";
		}

		// Save Paths
		public static class SavePath {
			public const string PlayerData = "user://player_data.nbt";
			public const string GameSettings = "user://game_settings.cfg";
		}
	}
}
