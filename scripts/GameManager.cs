// Entry point for Dangbox, needs to load up necessary assets + MainMenu instance
// Verify some client-side settings and preferences as well

using Godot;
using System.Collections.Generic;
using DangboxGame.Scripts.UI;
using System;

namespace DangboxGame.Scripts {
	public partial class GameManager : Node {
		public static GameManager Instance { get; private set; }

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
			
			// Subscribe to static events
			GameEvents.SceneLoaded += OnSceneLoaded;
			GameEvents.GameStarted += OnGameStarted;
			GameEvents.GamePaused += OnGamePaused;
			GameEvents.GameResumed += OnGameResumed;
			
			// Subscribe to new game control events
			GameEvents.GameStartRequested += OnGameStartRequested;
			GameEvents.GamePauseRequested += OnGamePauseRequested;
			GameEvents.GameResumeRequested += OnGameResumeRequested;
			GameEvents.MainMenuRequested += OnMainMenuRequested;
			GameEvents.QuitGameRequested += OnQuitGameRequested;
			
			GD.Print("GameManager initialized");
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

			GD.Print($"Game state initialized to: {targetState}");
		}

		private void ShowMainMenu() {
			Input.MouseMode = Input.MouseModeEnum.Visible;
			GameEvents.EmitUIStateChanged("main_menu");
		}

		public void StartGameSession() {
			_currentState = GameState.Loading;
			CleanupCurrentScene();
			
			GameEvents.EmitUIStateChanged("hud");
			
			// Use SceneService to load the game scene
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

		// New method: Join the game as a player (singleplayer or multiplayer)
		public void JoinGame() {
			if (_currentState != GameState.InGame) {
				GD.PrintErr("Cannot join game: Game is not in InGame state");
				return;
			}

			// Spawn local player with temporary coordinates
			Vector3 spawnPosition = new(0, 5, 0);
			GameEvents.EmitPlayerSpawnRequested(0, spawnPosition, Vector3.Zero);
			
			// Set mouse capture for gameplay
			Input.MouseMode = Input.MouseModeEnum.Captured;
		}

		private static void CleanupCurrentScene() {
			// Clean up all players when changing scenes
			Player.PlayerManager.Instance?.DespawnAllPlayers();
		}

		private void OnSceneLoaded(Node scene) {
			if (scene == null) {
				GD.PrintErr("Scene loaded event received with null scene");
				GD.PrintErr(System.Environment.StackTrace);
				throw new ArgumentNullException(nameof(scene), "Scene cannot be null");
			}

			GD.Print("Game scene loaded - ready for players to join");

			if (_currentState == GameState.Loading) {
				_currentState = GameState.InGame;
				GameEvents.EmitGameStarted();
				GD.Print("Game state set to InGame after scene load");
			}
		}

		private void OnGameStarted() {
			if (_currentState != GameState.InGame) {
				_currentState = GameState.InGame;
				GD.Print("Game state set to InGame");
			}

			JoinGame();
		}

		private void OnGamePaused() {
			_currentState = GameState.Paused;
		}

		private void OnGameResumed() {
			_currentState = GameState.InGame;
		}

		private void OnGameStartRequested() {
			StartGameSession();
		}

		private void OnGamePauseRequested() {
			if (_currentState == GameState.InGame) {
				UIManager.Instance?.ChangeUIState(UIManager.UIState.PauseMenu);
			}
		}

		private void OnGameResumeRequested() {
			if (_currentState == GameState.Paused) {
				UIManager.Instance?.ChangeUIState(UIManager.UIState.HUD);
			}
		}

		private void OnMainMenuRequested() {
			// Clean up and return to main menu
			CleanupCurrentScene();
			UIManager.Instance?.RestartToStartMenu();
		}

		private void OnQuitGameRequested() {
			GetTree().Quit();
		}

		public override void _ExitTree() {
			// Unsubscribe from events
			GameEvents.SceneLoaded -= OnSceneLoaded;
			GameEvents.GameStarted -= OnGameStarted;
			GameEvents.GamePaused -= OnGamePaused;
			GameEvents.GameResumed -= OnGameResumed;
			GameEvents.GameStartRequested -= OnGameStartRequested;
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
