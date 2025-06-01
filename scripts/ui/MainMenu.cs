using Godot;
using DangboxGame.Scripts;

namespace DangboxGame.Scripts.UI {
	public partial class MainMenu : Control {
		public override void _Ready() {
			GetNode<Button>("./VerticalContainer/StartButton").Connect("pressed", Callable.From(OnStartButtonPressed));
			GetNode<Button>("./VerticalContainer/SettingsButton").Connect("pressed", Callable.From(OnSettingsButtonPressed));
			GetNode<Button>("./VerticalContainer/QuitButton").Connect("pressed", Callable.From(OnQuitButtonPressed));
		}

		private void OnStartButtonPressed() {
			GameManager.Instance.StartHostGame();
		}

		private void OnSettingsButtonPressed() {
			UIManager.Instance.ChangeUIState(UIManager.UIState.SettingsMenu);
		}

		private void OnQuitButtonPressed() {
			GetTree().Quit();
		}
	}
}
