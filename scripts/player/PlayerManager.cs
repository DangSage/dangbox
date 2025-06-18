using Godot;
using System;
using System.Collections.Generic;

namespace DangboxGame.Scripts.Player {
	public partial class PlayerManager : Node {
		private readonly Dictionary<int, Node> _playerInstances = [];
		private readonly Dictionary<int, Node> _cameraInstances = [];

		public static PlayerManager Instance { get; private set; }
		
		[Signal]
		public delegate void PlayerReadyEventHandler(PlayerController player);
		
		public override void _Ready() {
			if (Instance != null && Instance != this) {
				QueueFree();
				return;
			}
			Instance = this;
			
			// Subscribe to game events
			GameEvents.PlayerSpawnRequested += OnPlayerSpawnRequested;
			GameEvents.AllPlayersDespawnRequested += DespawnAllPlayers;
			GD.Print("PlayerManager initialized as child of GameManager");

		}

		private void OnPlayerSpawnRequested(int playerId, Vector3 spawnPosition, Vector3 spawnRotation) {
			// Simplified - no complex validation since we know everything is ready
			try {
				SpawnPlayer(playerId, spawnPosition, spawnRotation);
			} catch (System.Exception e) {
				GD.PrintErr($"PlayerManager: Error spawning player {playerId}: {e.Message}");
			}
		}

		// Main method to spawn a player - simplified since validation happens earlier
		public int SpawnPlayer(int playerId, Vector3 spawnPosition, Vector3 spawnRotation) {
			try {
				// Detailed validation to identify the disposed object
				if (!IsInstanceValid(this)) {
					GD.PrintErr("PlayerManager: PlayerManager itself is disposed!");
					return -1;
				}
				
				if (!IsInsideTree()) {
					GD.PrintErr("PlayerManager: PlayerManager is not in tree!");
					return -1;
				}

				if (_playerInstances.ContainsKey(playerId)) {
					GD.Print($"Player {playerId} already exists");
					return playerId;
				}

				var playerInstance = InstantiateNode(Constants.PrefabPath.Player);
				if (playerInstance == null) {
					GD.PrintErr("PlayerManager: Failed to instantiate player");
					return -1;
				}

				if (!IsInstanceValid(playerInstance)) {
					GD.PrintErr("PlayerManager: Instantiated player is already disposed!");
					return -1;
				}

				_playerInstances[playerId] = playerInstance;

				var currentScene = SceneService.Instance.GetCurrentScene();
				currentScene.AddChild(playerInstance);

				if (playerInstance is Node3D playerNode) {
					playerNode.GlobalPosition = spawnPosition;
					playerNode.GlobalRotation = spawnRotation;
				}

				GD.Print($"Player {playerId} spawned successfully at {spawnPosition}");
				return playerId;
			} catch (System.Exception e) {
				GD.PrintErr($"PlayerManager: Error in SpawnPlayer for player {playerId}: {e.Message}");
				GD.PrintErr($"PlayerManager: Stack trace: {e.StackTrace}");
				return -1;
			}
		}

		public void DespawnPlayer(int playerId) {
			if (_playerInstances.TryGetValue(playerId, out Node player)) {
				player?.QueueFree();
				_playerInstances.Remove(playerId);
				GameEvents.EmitPlayerDespawned(playerId);
			}

			if (_cameraInstances.TryGetValue(playerId, out Node camera)) {
				camera?.QueueFree();
				_cameraInstances.Remove(playerId);
			}

			GD.Print($"Player {playerId} despawned");
		}

		// Clear all players (scene transition, etc.)
		public void DespawnAllPlayers() {
			foreach (var kvp in _playerInstances) {
				kvp.Value?.QueueFree();
			}
			_playerInstances.Clear();

			foreach (var kvp in _cameraInstances) {
				kvp.Value?.QueueFree();
			}
			_cameraInstances.Clear();
			
			GD.Print("All players despawned");
		}

		private static Node InstantiateNode(string scenePath) {
			try {
				if (!ResourceLoader.Exists(scenePath)) {
					GD.PrintErr($"PlayerManager: Scene file does not exist at {scenePath}");
					return null;
				}
				
				var packedScene = GD.Load<PackedScene>(scenePath);
				if (packedScene == null) {
					GD.PrintErr($"PlayerManager: Failed to load PackedScene from {scenePath}");
					return null;
				}
				
				var instance = packedScene.Instantiate();
				if (instance == null) {
					GD.PrintErr($"PlayerManager: Failed to instantiate scene from {scenePath}");
					return null;
				}
				
				return instance;
			} catch (System.Exception e) {
				GD.PrintErr($"PlayerManager: Error instantiating node from {scenePath}: {e.Message}");
				return null;
			}
		}

		public Node GetPlayer(int playerId) {
			return _playerInstances.TryGetValue(playerId, out Node player) ? player : null;
		}

		public Node GetLocalPlayer() {
			return GetPlayer(0);
		}

		public PlayerController GetLocalPlayerController() {
			var player = GetLocalPlayer();
			return player as PlayerController;
		}

		public override void _ExitTree() {
			GameEvents.PlayerSpawnRequested -= OnPlayerSpawnRequested;
			GameEvents.AllPlayersDespawnRequested -= DespawnAllPlayers;
			
			GD.Print("PlayerManager exiting tree");
		}
		
		private void OnPlayerInitialized(PlayerController player, int playerId) {
			GD.Print($"Player {playerId} initialized successfully");
			EmitSignal(SignalName.PlayerReady, player);
		}
	}
}
