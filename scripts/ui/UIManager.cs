using Godot;
using System.Collections.Generic;

namespace DangboxGame.Scripts.UI {
    public partial class UIManager : Node {
        public static UIManager Instance { get; private set; }

        // Layers
        private CanvasLayer _hudLayer;
        private CanvasLayer _menuLayer;

        // Active UI elements
        private Control _currentMenuUI;
        private Control _currentHUD;

        // Cached UI scenes
        private Dictionary<string, PackedScene> _cachedScenes = new();

        public override void _Ready() {
            if (Instance != null) {
                QueueFree();
                return;
            }
            Instance = this;

            InitializeLayers();
            PreloadUIScenes();
        }

        private void InitializeLayers() {
            // Create HUD layer (lower layer)
            _hudLayer = new CanvasLayer { 
                Name = "HUDLayer", 
                Layer = 1 
            };
            AddChild(_hudLayer);

            // Create menu layer (higher layer)
            _menuLayer = new CanvasLayer { 
                Name = "MenuLayer", 
                Layer = 2 
            };
            AddChild(_menuLayer);
        }

        private void PreloadUIScenes() {
            // Preload frequently used UI scenes
            CacheScene(Constants.ScenePath.MainMenu);
            CacheScene(Constants.ScenePath.PauseMenu);
            CacheScene(Constants.ScenePath.SettingsMenu);
            CacheScene(Constants.PrefabPath.HUD);
        }

        private void CacheScene(string path) {
            if (!_cachedScenes.ContainsKey(path)) {
                var scene = GD.Load<PackedScene>(path);
                if (scene != null) {
                    _cachedScenes[path] = scene;
                } else {
                    GD.PrintErr($"UIManager: Failed to cache scene: {path}");
                }
            }
        }

        public Control GetMainMenu() {
            return _currentMenuUI;
        }

        public void ShowMainMenu() {
            ShowMenu(Constants.ScenePath.MainMenu);
        }

        public void ShowPauseMenu() {
            ShowMenu(Constants.ScenePath.PauseMenu);
        }

        public void ShowSettingsMenu() {
            ShowMenu(Constants.ScenePath.SettingsMenu);
        }

        public void ShowHUD() {
            if (_currentHUD != null) return;

            _currentHUD = InstantiateUI(Constants.PrefabPath.HUD);
            _hudLayer.AddChild(_currentHUD);
        }

        public void HideHUD() {
            if (_currentHUD != null) {
                _currentHUD.QueueFree();
                _currentHUD = null;
            }
        }

        public void ShowMenu(string menuPath) {
            // Remove existing menu if any
            if (_currentMenuUI != null) {
                _currentMenuUI.QueueFree();
                _currentMenuUI = null;
            }

            // Instantiate and show new menu
            _currentMenuUI = InstantiateUI(menuPath);
            _menuLayer.AddChild(_currentMenuUI);
        }

        public void HideMenu() {
            if (_currentMenuUI != null) {
                _currentMenuUI.QueueFree();
                _currentMenuUI = null;
            }
        }

        private Control InstantiateUI(string path) {
            // Get or load scene
            if (!_cachedScenes.ContainsKey(path)) {
                CacheScene(path);
            }

            // Instantiate from cache
            if (_cachedScenes.ContainsKey(path) && _cachedScenes[path] != null) {
                return _cachedScenes[path].Instantiate<Control>();
            }

            GD.PrintErr($"UIManager: Failed to instantiate UI: {path}");
            return null;
        }
    }
}
