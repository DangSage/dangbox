using Godot;
using System.Collections.Generic;

namespace DangboxGame.Scripts {
	public partial class GameEvents : Node {
		private static GameEvents _instance;
		
		// Events - all static for global access
		public static event System.Action<string> UIStateChanged;
		public static event System.Action<string, Variant> SettingUpdated;
		public static event System.Action SettingsChanged;

		// Sequential Game Flow Events
		public static event System.Action GameStartRequested;
		public static event System.Action<Node> SceneLoaded;
		public static event System.Action ManagersReady;
		public static event System.Action SpawnPointsReady;
		public static event System.Action GameSessionReady;

		// Player Management Events
		public static event System.Action<int, Vector3, Vector3> PlayerSpawnRequested;
		public static event System.Action<int> PlayerDespawned;
		public static event System.Action AllPlayersDespawnRequested;

		// Game Control Events
		public static event System.Action GamePauseRequested;
		public static event System.Action GameResumeRequested;
		public static event System.Action MainMenuRequested;
		public static event System.Action QuitGameRequested;

		public override void _Ready() {
			if (_instance != null) {
				QueueFree();
				return;
			}
			_instance = this;
			ProcessMode = ProcessModeEnum.Always;
		}

		// Static emit methods - no instance needed
		public static void EmitUIStateChanged(string state) {
			UIStateChanged?.Invoke(state);
		}

		public static void EmitSettingUpdated(string settingName, Variant value) {
			SettingUpdated?.Invoke(settingName, value);
		}

		public static void EmitSettingsChanged() {
			SettingsChanged?.Invoke();
		}

		// Sequential Game Flow Events
		public static void EmitGameStartRequested() {
			GameStartRequested?.Invoke();
		}

		public static void EmitSceneLoaded(Node scene) {
			SceneLoaded?.Invoke(scene);
		}

		public static void EmitManagersReady() {
			ManagersReady?.Invoke();
		}

		public static void EmitSpawnPointsReady() {
			SpawnPointsReady?.Invoke();
		}

		public static void EmitGameSessionReady() {
			GameSessionReady?.Invoke();
		}

		// Player Management Events
		public static void EmitPlayerSpawnRequested(int playerId, Vector3 spawnPosition = default, Vector3 spawnRotation = default) {
			PlayerSpawnRequested?.Invoke(playerId, spawnPosition, spawnRotation);
		}

		public static void EmitPlayerDespawned(int playerId) {
			PlayerDespawned?.Invoke(playerId);
		}

		public static void EmitAllPlayersDespawnRequested() {
			AllPlayersDespawnRequested?.Invoke();
		}

		// Game Control Events
		public static void EmitGamePauseRequested() {
			GamePauseRequested?.Invoke();
		}

		public static void EmitGameResumeRequested() {
			GameResumeRequested?.Invoke();
		}

		public static void EmitMainMenuRequested() {
			MainMenuRequested?.Invoke();
		}

		public static void EmitQuitGameRequested() {
			QuitGameRequested?.Invoke();
		}

		public override void _ExitTree() {
			if (_instance == this) {
				_instance = null;
			}
		}
	}
}
