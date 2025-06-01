using Godot;
using System.Collections.Generic;

namespace DangboxGame.Scripts {
	public partial class GameEvents : Node {
		private static GameEvents _instance;
		
		// Events - all static for global access
		public static event System.Action<string> UIStateChanged;
		public static event System.Action<string, Variant> SettingUpdated;
		public static event System.Action SettingsChanged;
		public static event System.Action<Node> SceneLoaded;
		public static event System.Action<string> SceneChangeRequested;
		public static event System.Action GameStarted;
		public static event System.Action GamePaused;
		public static event System.Action GameResumed;

		// Player Management Events
		public static event System.Action<int, Vector3, Vector3> PlayerSpawnRequested;
		public static event System.Action<int, Vector3, Vector3> PlayerSpawned;
		public static event System.Action<int> PlayerDespawnRequested;
		public static event System.Action<int> PlayerDespawned;
		public static event System.Action LocalPlayerSpawnRequested;
		public static event System.Action<int> LocalPlayerSpawned;
		public static event System.Action AllPlayersDespawnRequested;
		public static event System.Action<List<int>> AllPlayersDespawned;

		// Game Control Events
		public static event System.Action GameStartRequested;
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

		public static void EmitSceneLoaded(Node scene) {
			SceneLoaded?.Invoke(scene);
		}

		public static void EmitSceneChangeRequested(string scenePath) {
			SceneChangeRequested?.Invoke(scenePath);
		}

		public static void EmitGameStarted() {
			GameStarted?.Invoke();
		}

		public static void EmitGamePaused() {
			GamePaused?.Invoke();
		}

		public static void EmitGameResumed() {
			GameResumed?.Invoke();
		}

		// Player Management Events
		public static void EmitPlayerSpawnRequested(int playerId, Vector3 spawnPosition = default, Vector3 spawnRotation = default) {
			PlayerSpawnRequested?.Invoke(playerId, spawnPosition, spawnRotation);
		}

		public static void EmitPlayerSpawned(int playerId, Vector3 position, Vector3 rotation) {
			PlayerSpawned?.Invoke(playerId, position, rotation);
		}

		public static void EmitPlayerDespawnRequested(int playerId) {
			PlayerDespawnRequested?.Invoke(playerId);
		}

		public static void EmitPlayerDespawned(int playerId) {
			PlayerDespawned?.Invoke(playerId);
		}

		public static void EmitLocalPlayerSpawnRequested() {
			LocalPlayerSpawnRequested?.Invoke();
		}

		public static void EmitLocalPlayerSpawned(int playerId) {
			LocalPlayerSpawned?.Invoke(playerId);
		}

		public static void EmitAllPlayersDespawnRequested() {
			AllPlayersDespawnRequested?.Invoke();
		}

		public static void EmitAllPlayersDespawned(List<int> playerIds) {
			AllPlayersDespawned?.Invoke(playerIds);
		}

		// Game Control Events
		public static void EmitGameStartRequested() {
			GameStartRequested?.Invoke();
		}

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
