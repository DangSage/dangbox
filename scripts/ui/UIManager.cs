using Godot;
using System.Collections.Generic;

namespace DangboxGame.Scripts.UI {
    public partial class UIManager : Node {
        public static UIManager Instance { get; private set; }

        // Layers
        private CanvasLayer _hudLayer;
        private CanvasLayer _menuLayer;

        // Active UI elements
        private Control _currentUI;

        // Enum for UI states
        public enum UIState {
            MainMenu,
            PauseMenu,
            SettingsMenu,
            HUD
        }

        // Cached UI scenes
        private Dictionary<UIState, PackedScene> _cachedScenes = new();

        private UIState _currentState;

        public override void _Ready() {
            if (Instance != null) {
                QueueFree();
                return;
            }
            Instance = this;

            InitializeLayers();
            PreloadUIScenes();
            _currentState = UIState.MainMenu; // Default state
            ChangeUIState(_currentState); // Initialize UI
        }

        private void InitializeLayers() {
            _hudLayer = new CanvasLayer { Name = "HUDLayer", Layer = 1 };
            AddChild(_hudLayer);

            _menuLayer = new CanvasLayer { Name = "MenuLayer", Layer = 2 };
            AddChild(_menuLayer);
        }

        private void PreloadUIScenes() {
            CacheScene(UIState.MainMenu, Constants.ScenePath.MainMenu);
            CacheScene(UIState.PauseMenu, Constants.ScenePath.PauseMenu);
            CacheScene(UIState.SettingsMenu, Constants.ScenePath.SettingsMenu);
            CacheScene(UIState.HUD, Constants.PrefabPath.HUD);
        }

        private void CacheScene(UIState state, string path) {
            if (!_cachedScenes.ContainsKey(state)) {
                var scene = GD.Load<PackedScene>(path);
                if (scene != null) {
                    _cachedScenes[state] = scene;
                } else {
                    GD.PrintErr($"UIManager: Failed to cache scene for state {state}: {path}");
                }
            }
        }

        public void ChangeUIState(UIState newState) {
            if (_currentState == newState) return;

            // Remove current UI
            if (_currentUI != null) {
                _currentUI.QueueFree();
                _currentUI = null;
            }

            // Instantiate and show new UI
            if (_cachedScenes.TryGetValue(newState, out var scene)) {
                _currentUI = scene.Instantiate<Control>();
                if (newState == UIState.HUD) {
                    _hudLayer.AddChild(_currentUI);
                } else {
                    _menuLayer.AddChild(_currentUI);
                }
            } else {
                GD.PrintErr($"UIManager: No cached scene for state {newState}");
            }

            _currentState = newState;
        }

        public UIState GetCurrentState() {
            return _currentState;
        }

        // Entry point for external scripts to change UI state
        public void SetUIStateFromButton(string stateName) {
            if (System.Enum.TryParse(stateName, out UIState newState)) {
                ChangeUIState(newState);
            } else {
                GD.PrintErr($"UIManager: Invalid UIState '{stateName}'");
            }
        }
    }
}
