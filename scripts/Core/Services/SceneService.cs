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

            GameEvents.SceneChangeRequested += LoadScene;
        }

        public void LoadScene(string scenePath) {
            if (string.IsNullOrEmpty(scenePath)) {
                throw new System.ArgumentException("Scene path cannot be null or empty.", nameof(scenePath));
            }

            var packedScene = GD.Load<PackedScene>(scenePath);
            if (packedScene == null) {
                throw new System.IO.FileNotFoundException($"PackedScene not found at path: {scenePath}", scenePath);
            }

            // Store reference to old scene before changing
            var oldScene = _tree.CurrentScene;

            // Free the old scene (this will happen after the current frame)
            if (oldScene != null && oldScene != this) {
                oldScene.QueueFree();
            }

            // Instantiate new scene
            var newScene = packedScene.Instantiate();
            if (newScene == null) {
                throw new System.InvalidOperationException($"Failed to instantiate scene from path: {scenePath}");
            }

            // Add the new scene to the tree as a child of root
            _tree.Root.AddChild(newScene);
            _tree.CurrentScene = newScene;
            _currentScene = newScene;

            // Emit the scene loaded event with the actual scene
            GameEvents.EmitSceneLoaded(newScene);

            GD.Print($"Scene loaded successfully: {scenePath}");
        }

        public Node GetCurrentScene() {
            return _currentScene;
        }

        public override void _ExitTree() {
            GameEvents.SceneChangeRequested -= LoadScene;

            if (Instance == this) {
                Instance = null;
            }
        }
    }
}
