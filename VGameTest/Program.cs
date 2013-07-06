using System;
using VGame;
using Cairo;

namespace VGameTest {
	class TestGame : Game {
		public override void Initialize() {
			ScreenManager.AddScreen(new TestScreen());
		}
	}
	class TestScreen : Screen {
		public override void Draw(Context g) {
			g.MoveTo(20, 20);
			g.LineTo(120, 20);
			g.LineTo(120, 120);
			g.LineTo(20, 120);
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
