using Godot;
using System.Threading.Tasks;

namespace DangboxGame.Scripts {
	public partial class SceneService : Node {
		public static SceneService Instance { get; private set; }

		private Node _currentScene;
		private SceneTree _tree;

		public override void _Ready() {
			Instance = this;
			_tree = GetTree();
			_currentScene = _tree.CurrentScene;

			// Ensure this service persists across scene changes
			ProcessMode = ProcessModeEnum.Always;
			
			// Move to root to ensure persistence
			var root = _tree.Root;
			if (GetParent() != root) {
				GetParent()?.RemoveChild(this);
				root.AddChild(this);
			}
		}

		public void LoadScene(string scenePath) {
			if (string.IsNullOrEmpty(scenePath)) {
				throw new System.ArgumentException("Scene path cannot be null or empty.", nameof(scenePath));
			}

			var packedScene = GD.Load<PackedScene>(scenePath);
			if (packedScene == null) {
				throw new System.IO.FileNotFoundException($"PackedScene not found at path: {scenePath}", scenePath);
			}

			var oldScene = _tree.CurrentScene;
			var newScene = packedScene.Instantiate();
			if (newScene == null) {
				throw new System.InvalidOperationException($"Failed to instantiate scene from path: {scenePath}");
			}

			// Add new scene and set as current
			_tree.Root.AddChild(newScene);
			_tree.CurrentScene = newScene;
			_currentScene = newScene;

			// Clean up old scene
			if (oldScene != null && oldScene != this && IsInstanceValid(oldScene)) {
				oldScene.QueueFree();
			}

			GD.Print($"Scene loaded: {scenePath}");
			
			// Emit scene loaded immediately - validation happens in GameManager
			GameEvents.EmitSceneLoaded(newScene);
		}

		public Node GetCurrentScene() {
			return _currentScene;
		}

		public override void _ExitTree() {
			if (Instance == this) {
				Instance = null;
			}
		}
	}
}
