using Godot;
using System;
using System.Collections.Generic;

namespace DangboxGame.Scripts.Player {
	public partial class PlayerManager : Node {
		public static PlayerManager Instance { get; private set; }

		private readonly Dictionary<int, Node> _playerInstances = [];
		private readonly Dictionary<int, Node> _cameraInstances = [];

		public override void _Ready() {
			if (Instance != null) {
				QueueFree();
				return;
			}
			Instance = this;
			ProcessMode = ProcessModeEnum.Always;

			GameEvents.PlayerSpawnRequested += OnPlayerSpawnRequested;
			GameEvents.PlayerDespawnRequested += DespawnPlayer;
			GameEvents.LocalPlayerSpawnRequested += SpawnLocalPlayer;
			GameEvents.AllPlayersDespawnRequested += DespawnAllPlayers;

			GD.Print("PlayerManager initialized");
		}

		private void OnPlayerSpawnRequested(int playerId, Vector3 spawnPosition, Vector3 spawnRotation) {
			try {
				if (!IsInstanceValid(this) || !IsInsideTree()) {
					GD.PrintErr("PlayerManager: Cannot spawn player - manager is not in tree");
					return;
				}
				SpawnPlayer(playerId, spawnPosition, spawnRotation);
			} catch (System.Exception e) {
				GD.PrintErr($"PlayerManager: Error spawning player {playerId}: {e.Message}");
			}
		}

		// Main method to spawn a player - works for both singleplayer and multiplayer
		public int SpawnPlayer(int playerId, Vector3 spawnPosition, Vector3 spawnRotation) {
			try {
				if (!IsInstanceValid(this) || !IsInsideTree()) {
					GD.PrintErr("PlayerManager: Cannot spawn player - manager is not valid or in tree");
					return -1;
				}

				if (_playerInstances.ContainsKey(playerId)) {
					GD.Print($"Player {playerId} already exists");
					return playerId;
				}

				var playerInstance = InstantiateNode(Constants.PrefabPath.Player);
				if (playerInstance == null) {
					GD.PrintErr("PlayerManager: Failed to instantiate player - playerInstance is null");
					return -1;
				}

				_playerInstances[playerId] = playerInstance;
				if (playerId == 0) {
					SpawnPlayerInput(playerInstance);
					SpawnCamera(playerId);
				}
				
				AddChild(playerInstance);

				if (playerInstance is Node3D playerNode) {
					playerNode.GlobalPosition = spawnPosition;
					playerNode.GlobalRotation = spawnRotation;
				} else {
					GD.PrintErr("PlayerManager: Player instance is not a Node3D!");
				}

				GD.Print($"Player {playerId} spawned successfully at {spawnPosition}");
				return playerId;
			} catch (System.Exception e) {
				GD.PrintErr($"PlayerManager: Error in SpawnPlayer for player {playerId}: {e.Message}");
				return -1;
			}
		}

		public void SpawnLocalPlayer() {
			int playerId = SpawnPlayer(0, GetSpawnPosition(), Vector3.Zero);
			if (playerId >= 0) {
				GameEvents.EmitLocalPlayerSpawned(playerId);
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
			var playerIds = new List<int>(_playerInstances.Keys);
			
			foreach (var kvp in _playerInstances) {
				kvp.Value?.QueueFree();
			}
			_playerInstances.Clear();

			foreach (var kvp in _cameraInstances) {
				kvp.Value?.QueueFree();
			}
			_cameraInstances.Clear();
			
			GameEvents.EmitAllPlayersDespawned(playerIds);
			GD.Print("All players despawned");
		}

		private static void SpawnPlayerInput(Node playerInstance) {
			var playerInput = new Node { Name = "_PlayerInput" };
			var inputScript = GD.Load<Script>(Constants.ScriptPath.PlayerInput);
			if (inputScript != null) {
				playerInput.SetScript(inputScript);
				playerInstance.AddChild(playerInput);
			} else {
				GD.PrintErr($"Failed to load PlayerInput script from {Constants.ScriptPath.PlayerInput}");
			}
		}

		private void SpawnCamera(int playerId) {
			try {
				var cameraInstance = InstantiateNode(Constants.PrefabPath.Camera);
				if (cameraInstance != null) {
					var currentScene = GetCurrentSceneSafely();
					if (currentScene == null) {
						GD.PrintErr("PlayerManager: Cannot spawn camera - no valid current scene");
						return;
					}

					currentScene.AddChild(cameraInstance);
					_cameraInstances[playerId] = cameraInstance;

					if (GameSettings.Instance != null) {
						var camera = cameraInstance as Camera3D;
						if (camera != null) {
							camera.Fov = GameSettings.Instance.GetFOV();
						}
					} else {
						GD.PrintErr("GameSettings instance not found, cannot set camera FOV");
					}

					GD.Print($"Camera spawned for player {playerId}");
				}
			} catch (System.Exception e) {
				GD.PrintErr($"PlayerManager: Error spawning camera for player {playerId}: {e.Message}");
			}
		}

		private static Vector3 GetSpawnPosition() {
			try {
				var currentScene = GetCurrentSceneSafely();
				if (currentScene != null) {
					var spawnPoint = currentScene.GetNodeOrNull<Node3D>("PlayerSpawn");
					if (spawnPoint != null) {
						return spawnPoint.GlobalPosition;
					}
				}
				return Vector3.Zero;
			} catch (System.Exception e) {
				GD.PrintErr($"PlayerManager: Error getting spawn position: {e.Message}");
				return Vector3.Zero;
			}
		}

		private static Node GetCurrentSceneSafely() {
			try {
				if (SceneService.Instance == null || !IsInstanceValid(SceneService.Instance)) {
					GD.PrintErr("PlayerManager: SceneService instance is null or invalid");
					return null;
				}

				var currentScene = SceneService.Instance.GetCurrentScene();
				if (currentScene == null || !IsInstanceValid(currentScene)) {
					GD.PrintErr("PlayerManager: Current scene is null or invalid");
					return null;
				}

				return currentScene;
			} catch (System.Exception e) {
				GD.PrintErr($"PlayerManager: Error getting current scene safely: {e.Message}");
				return null;
			}
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

		public override void _ExitTree() {
			GameEvents.PlayerSpawnRequested -= OnPlayerSpawnRequested;
			GameEvents.PlayerDespawnRequested -= DespawnPlayer;
			GameEvents.LocalPlayerSpawnRequested -= SpawnLocalPlayer;
			GameEvents.AllPlayersDespawnRequested -= DespawnAllPlayers;
			
			if (Instance == this) {
				Instance = null;
			}
		}
	}
}
