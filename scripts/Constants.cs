using Godot;

namespace DangboxGame.Scripts {
	public static class Constants {
		// Scene Paths
		public static class ScenePath {
			public const string MainMenu = "res://scenes/ui/MainScreen.tscn";
			public const string SettingsMenu = "res://scenes/ui/SettingsMenu.tscn";
			public const string PauseMenu = "res://scenes/ui/PauseMenu.tscn";
			public const string TestLevel = "res://scenes/TestLevel.tscn";
			public const string MainGameScene = "res://scenes/Main.tscn";
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
	}
}
