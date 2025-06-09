// Entry point for Dangbox, needs to load up necessary assets + MainMenu instance
// Verify some client-side settings and preferences as well

using Godot;
using System.Collections.Generic;
using DangboxGame.Scripts.UI;
using DangboxGame.Scripts.Core.Environment;
using System;

namespace DangboxGame.Scripts {
	public partial class GameManager : Node {
		public static GameManager Instance { get; private set; }
		
		public GameSettings Settings { get; private set; }
		public Player.PlayerManager PlayerManager { get; private set; }

		public enum GameState {
			MainMenu,
			Loading,
			InGame,
			Paused
		}

		private GameState _currentState = GameState.MainMenu;

		public override void _Ready() {
			if (Instance != null) {
				QueueFree();
				return;
			}
			Instance = this;
			ProcessMode = ProcessModeEnum.Always;
			
			Settings = new GameSettings();
			AddChild(Settings);
			
			GameEvents.GameStartRequested += OnGameStartRequested;
			GameEvents.SceneLoaded += OnSceneLoaded;
			GameEvents.ManagersReady += OnManagersReady;
			GameEvents.SpawnPointsReady += OnSpawnPointsReady;
			GameEvents.GameSessionReady += OnGameSessionReady;
			GameEvents.GamePauseRequested += OnGamePauseRequested;
			GameEvents.GameResumeRequested += OnGameResumeRequested;
			GameEvents.MainMenuRequested += OnMainMenuRequested;
			GameEvents.QuitGameRequested += OnQuitGameRequested;
		}

		public void InitializeGameState(GameState targetState = GameState.MainMenu) {
			_currentState = targetState;

			switch (targetState) {
				case GameState.MainMenu:
					ShowMainMenu();
					break;
				case GameState.InGame:
					StartGameSession();
					break;
			}
		}

		private void ShowMainMenu() {
			Input.MouseMode = Input.MouseModeEnum.Visible;
			GameEvents.EmitUIStateChanged("main_menu");
		}

		public void StartGameSession() {
			_currentState = GameState.Loading;
			
			GameEvents.EmitUIStateChanged("hud");
			
			string scenePath = Constants.ScenePath.MainGameScene;
			if (!ResourceLoader.Exists(scenePath)) {
				GD.PrintErr($"Main game scene not found: {scenePath}");
				scenePath = Constants.ScenePath.TestLevel;
				if (!ResourceLoader.Exists(scenePath)) {
					GD.PrintErr($"Test level scene not found: {scenePath}");
					ShowMainMenu();
					return;
				}
			}
			
			SceneService.Instance?.LoadScene(scenePath);
		}

		public void JoinGame() {
			if (_currentState != GameState.InGame) {
				GD.PrintErr("Cannot join game: Game is not in InGame state");
				return;
			}

			Vector3 spawnPosition = new(0, 0, 0);
			GameEvents.EmitPlayerSpawnRequested(0, spawnPosition, Vector3.Zero);
		}

		private void OnGameStartRequested() {
			_currentState = GameState.Loading;
			GameEvents.EmitUIStateChanged("hud");
			
			string scenePath = Constants.ScenePath.MainGameScene;
			if (!ResourceLoader.Exists(scenePath)) {
				GD.PrintErr($"Main game scene not found: {scenePath}");
				scenePath = Constants.ScenePath.TestLevel;
				if (!ResourceLoader.Exists(scenePath)) {
					GD.PrintErr($"Test level scene not found: {scenePath}");
					ShowMainMenu();
					return;
				}
			}
			
			SceneService.Instance?.LoadScene(scenePath);
		}

		private void OnSceneLoaded(Node scene) {
			if (scene == null) {
				GD.PrintErr("Scene loaded event received with null scene");
				ShowMainMenu();
				return;
			}

			GD.Print("Scene loaded successfully, creating PlayerManager...");
			
			if (scene.Name == "Node" || scene.GetType().Name == "Node") {
				CreatePlayerManager();
			}
			
			if (ValidateManagersReady()) {
				GameEvents.EmitManagersReady();
			} else {
				GD.PrintErr("Managers not ready after scene load");
				ShowMainMenu();
			}
		}

		private void CreatePlayerManager() {
			if (PlayerManager != null && IsInstanceValid(PlayerManager)) {
				PlayerManager.QueueFree();
				PlayerManager = null;
			}

			PlayerManager = new Player.PlayerManager();
			PlayerManager.Name = "PlayerManager";
			AddChild(PlayerManager);
			
			GD.Print("PlayerManager created as child of GameManager");
		}

		private void CleanupPlayerManager() {
			if (PlayerManager != null && IsInstanceValid(PlayerManager)) {
				PlayerManager.QueueFree();
				PlayerManager = null;
				GD.Print("PlayerManager cleaned up");
			}
		}

		private void OnManagersReady() {
			GD.Print("Managers ready, validating spawn points...");
			
			if (ValidateSpawnPointsReady()) {
				GameEvents.EmitSpawnPointsReady();
			} else {
				GD.PrintErr("Spawn points not ready");
				ShowMainMenu();
			}
		}

		private void OnSpawnPointsReady() {
			GD.Print("Spawn points ready, game session is ready");
			GameEvents.EmitGameSessionReady();
		}

		private void OnGameSessionReady() {
			_currentState = GameState.InGame;
			GD.Print("Game session ready - starting player spawn");
			
			if (!ValidateManagersReady()) {
				GD.PrintErr("Managers became invalid just before player spawn!");
				ShowMainMenu();
				return;
			}
			
			Vector3 spawnPosition = GetSpawnPosition();
			GameEvents.EmitPlayerSpawnRequested(0, spawnPosition, Vector3.Zero);
		}

		private bool ValidateManagersReady() {
			bool sceneServiceReady = SceneService.Instance != null && 
									IsInstanceValid(SceneService.Instance) && 
									SceneService.Instance.IsInsideTree();
			if (!sceneServiceReady) {
				GD.PrintErr($"SceneService validation failed");
			}
			
			bool playerManagerReady = true;
			var currentScene = SceneService.Instance?.GetCurrentScene();
			if (currentScene != null && (currentScene.Name == "Node" || currentScene.GetType().Name == "Node")) {
				playerManagerReady = PlayerManager != null &&
									IsInstanceValid(PlayerManager) &&
									PlayerManager.IsInsideTree();
				if (!playerManagerReady) {
					GD.PrintErr($"PlayerManager validation failed");
				}
			}
			
			bool gameSettingsReady = Settings != null && IsInstanceValid(Settings);
			if (!gameSettingsReady) {
				GD.PrintErr($"GameSettings validation failed");
			}
			
			bool uiManagerReady = UIManager.Instance != null &&
								 IsInstanceValid(UIManager.Instance) &&
								 UIManager.Instance.IsInsideTree();
			if (!uiManagerReady) {
				GD.PrintErr($"UIManager validation failed");
			}

			return sceneServiceReady && playerManagerReady && gameSettingsReady && uiManagerReady;
		}

		private bool ValidateSpawnPointsReady() {
			var currentScene = SceneService.Instance?.GetCurrentScene();
			if (currentScene == null || !IsInstanceValid(currentScene)) {
				return false;
			}

			var spawnPoint = currentScene.GetNodeOrNull<Node3D>("PlayerSpawn");
			return spawnPoint != null && IsInstanceValid(spawnPoint);
		}

		private Vector3 GetSpawnPosition() {
			var currentScene = SceneService.Instance?.GetCurrentScene();
			if (currentScene != null) {
				var spawnPoint = currentScene.GetNodeOrNull<Node3D>("PlayerSpawn");
				if (spawnPoint != null) {
					return spawnPoint.GlobalPosition;
				}
			}
			return new Vector3(0, 5, 0);
		}

		private void OnGamePauseRequested() {
			if (_currentState == GameState.InGame) {
				_currentState = GameState.Paused;
				UIManager.Instance?.ChangeUIState(UIManager.UIState.PauseMenu);
			}
		}

		private void OnGameResumeRequested() {
			if (_currentState == GameState.Paused) {
				_currentState = GameState.InGame;
				UIManager.Instance?.ChangeUIState(UIManager.UIState.HUD);
			}
		}

		private void OnMainMenuRequested() {
			// Ensure player input is re-enabled before cleanup
			GameEvents.EmitPlayerInputEnabled(true);
			CleanupPlayerManager();
			UIManager.Instance?.RestartToStartMenu();
		}

		private void OnQuitGameRequested() {
			// Ensure player input is re-enabled before cleanup
			GameEvents.EmitPlayerInputEnabled(true);
			CleanupPlayerManager();
			Settings?.SaveSettings();
			EnvironmentPaths.WriteLog("shutdown", $"Game shutdown at {System.DateTime.Now}");
			GD.Print("Quitting game...");
			GetTree().Quit();
		}

		public override void _ExitTree() {
			GameEvents.GameStartRequested -= OnGameStartRequested;
			GameEvents.SceneLoaded -= OnSceneLoaded;
			GameEvents.ManagersReady -= OnManagersReady;
			GameEvents.SpawnPointsReady -= OnSpawnPointsReady;
			GameEvents.GameSessionReady -= OnGameSessionReady;
			GameEvents.GamePauseRequested -= OnGamePauseRequested;
			GameEvents.GameResumeRequested -= OnGameResumeRequested;
			GameEvents.MainMenuRequested -= OnMainMenuRequested;
			GameEvents.QuitGameRequested -= OnQuitGameRequested;

			if (Instance == this) {
				Instance = null;
			}
		}

		public GameState CurrentState => _currentState;
	}
}
