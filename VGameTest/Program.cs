using System;
using System.Linq;
using Tao.Sdl;
using Cairo;
using VGame;

namespace VGameTest {
	class TestGame : Game {
		public override void Initialize() {
			CursorVisible = false;
			ConstrainMouse = true;
			StateManager.AddState(new TestMenu());
		}
	}
	class TestState : State {
		VGame.Shapes.Cursor cursor = new VGame.Shapes.Cursor();
		VGame.Point playerPosition = new VGame.Point();
		public override void Update() {
			playerPosition = InputManager.MousePosition;
			if (InputManager.MouseButtonState(MouseButton.Left) == ButtonState.Pressed)
				Game.ConstrainMouse = !Game.ConstrainMouse;
			if (InputManager.MouseButtonState(MouseButton.Right) == ButtonState.Pressed)
				Game.CursorVisible = !Game.CursorVisible;
		}
		public override void Draw() {
			Context g = Renderer.Context;
			Renderer.Clear(ColorPresets.LightSteelBlue);
			Renderer.DrawText(new Vector2(Renderer.Width / 2, Renderer.Height / 2), "LEFT CLICK TO UNLOCK THE MOUSE FROM THE SCREEN, RIGHT CLICK TO TOGGLE THE CURSOR", 24, TextAlign.Center, TextAlign.Middle, new Color(1, 1, 1), new Color(0, 0, 0), null, 0, null);
			cursor.Draw(g, new Vector2(playerPosition.X, playerPosition.Y), 0, ColorPresets.White, ColorPresets.Black, 24);
		}
	}
	class TestMenu : Menu {
		public TestMenu() : base("test menu") {
			Entries.Add(new MenuEntry(this, "test1"));
			Entries.Last().Selected += delegate(object sender, EventArgs e) {
				Console.WriteLine("test1 clicked!");
				StateManager.AddState(new TestState());
			};
			Entries.Add(new MenuEntry(this, "test2"));
			Entries.Last().Selected += delegate(object sender, EventArgs e) {
				Console.WriteLine("test2 clicked!");
			};
			Entries.Add(new SpacerEntry(this));
			Entries.Add(new MenuEntry(this, "test3"));
			Entries.Last().Selected += delegate(object sender, EventArgs e) {
				Console.WriteLine("test3 clicked!");
			};
			Entries.Add(new MenuEntry(this, "test4"));
			Entries.Last().Selected += delegate(object sender, EventArgs e) {
				Console.WriteLine("test4 clicked!");
			};
			Entries.Add(new SpacerEntry(this));
			Entries.Add(new MenuEntry(this, "QUIT"));
			Entries.Last().Selected += delegate(object sender, EventArgs e) {
				OnCancel();
			};
		}
		protected override void OnCancel() {
			Game.Exit();
		}
	}
	class Program {
		public static void Main(string[] args) {
			Game game = new TestGame();
			game.Run();
		}
	}
}
