using System;
using Tao.Sdl;
using Cairo;
using VGame;

namespace VGameTest {
	class TestGame : Game {
		public override void Initialize() {
			StateManager.AddState(new TestState());
		}
	}
	class TestState : State {
		VGame.Point playerPosition = new VGame.Point();
		public override void Initialize() {
			InputManager.CursorVisible = false;
			InputManager.ConstrainMouse = true;
		}
		public override void Update() {
			playerPosition = InputManager.MousePosition;
			if (InputManager.MouseButtonState(MouseButton.Left) != ButtonState.Up && InputManager.MouseButtonState(MouseButton.Left) != ButtonState.Down)
				Console.WriteLine(InputManager.MouseButtonState(MouseButton.Left));
		}
		public override void Draw(Renderer r, Context g) {
			r.DrawText(g, new Vector2(0, 0), "TEST", 24, TextAlign.Left, TextAlign.Top, new Color(1, 1, 1), new Color(0, 0, 0), null, 0, null);
			g.MoveTo(playerPosition.X - 10, playerPosition.Y - 10);
			g.LineTo(playerPosition.X + 10, playerPosition.Y - 10);
			g.LineTo(playerPosition.X + 10, playerPosition.Y + 10);
			g.LineTo(playerPosition.X - 10, playerPosition.Y + 10);
			g.ClosePath();
			g.Color = new Color(1, 1, 1);
			g.FillPreserve();
			g.Color = new Color(0, 0, 0);
			g.Stroke();
		}
	}
	class Program {
		public static void Main(string[] args) {
			Game game = new TestGame();
			game.Run();
		}
	}
}
