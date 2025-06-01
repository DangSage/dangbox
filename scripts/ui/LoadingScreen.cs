using Godot;
using System;

namespace DangboxGame.Scripts.UI {
	public partial class LoadingScreen : VBoxContainer {
		private ProgressBar _progressBar;
		private Label _statusLabel;
		private TextureRect _logo;
		
		// Animation properties
		private float _targetProgress = 0f;
		private float _currentProgress = 0f;
		private bool _isLoading = false;
		private Tween _logoTween;

		public override void _Ready() {
			_progressBar = GetNodeOrNull<ProgressBar>("ProgressBar");
			_statusLabel = GetNodeOrNull<Label>("StatusLabel");
			_logo = GetNodeOrNull<TextureRect>("logo");
		}

		public override void _Process(double delta) {
			if (!_isLoading) return;

			// Smooth progress animation with threshold to snap to target
			float lerpSpeed = (float)(delta * 8.0);
			_currentProgress = Mathf.Lerp(_currentProgress, _targetProgress, lerpSpeed);
			
			// Snap to target if very close to avoid floating point precision issues
			if (Mathf.Abs(_currentProgress - _targetProgress) < 0.01f) {
				_currentProgress = _targetProgress;
			}
			
			if (_progressBar != null) {
				_progressBar.Value = _currentProgress * 100;
			}

			if (_logo != null) {
				_logo.PivotOffset = new Vector2(_logo.Size.X / 2, _logo.Size.Y / 2); // Center pivot
				_logo.Rotation = _currentProgress * Mathf.Tau; // Full 360° rotation based on progress
			}
		}

		public void StartLoading(string initialMessage = "Loading...") {
			_isLoading = true;
			_currentProgress = 0f;
			_targetProgress = 0f;
			
			if (_statusLabel != null) {
				_statusLabel.Text = initialMessage;
			}
			
			if (_progressBar != null) {
				_progressBar.Value = 0;
				_progressBar.Modulate = Colors.White;
			}
		}

		public void UpdateProgress(float progress, string message = "") {
			_targetProgress = Mathf.Clamp(progress, 0f, 1f);
			
			if (!string.IsNullOrEmpty(message) && _statusLabel != null) {
				_statusLabel.Text = message;
			}
		}

		public void CompleteLoading(string completionMessage = "Complete!") {
			_targetProgress = 1f;
			_currentProgress = 1f; // Force immediate completion
			_isLoading = false;
			
			if (_statusLabel != null) {
				_statusLabel.Text = completionMessage;
				_statusLabel.RemoveThemeColorOverride("font_color"); // Reset to default color
			}
			
			if (_progressBar != null) {
				_progressBar.Value = 100; // Explicitly set to 100%
				_progressBar.Modulate = Colors.LightGreen;
			}
			
			if (_logo != null) {
				_logo.Rotation = Mathf.Tau; // Ensure full 360° rotation
			}
		}

		public void ShowError(string errorMessage) {
			_isLoading = false;
			
			if (_statusLabel != null) {
				_statusLabel.Text = errorMessage;
				_statusLabel.AddThemeColorOverride("font_color", Colors.Red);
			}
			
			if (_progressBar != null) {
				_progressBar.Modulate = Colors.Red;
				_progressBar.Value = 0; // Reset progress bar to show error state
			}
		}

		public override void _ExitTree() {
			_logoTween?.Kill();
			base._ExitTree();
		}
	}
}
