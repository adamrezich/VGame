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
		int x = 0;
		int y = 0;
		public override void Update() {
			Console.WriteLine("UPDATE");
			Sdl.SDL_GetMouseState(out x, out y);
		}
		public override void Draw(Context g) {
			g.MoveTo(x - 10, y - 10);
			g.LineTo(x + 10, y - 10);
			g.LineTo(x + 10, y + 10);
			g.LineTo(x - 10, y + 10);
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
