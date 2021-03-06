using System;
using System.Linq;
using Tao.Sdl;
using Cairo;
using VGame;

using VGame.StateSystem;

namespace VGameTest {
	public class TestGame : Game {
		public FreeTypeFontFace Font;
		protected override void Initialize() {
			CursorVisible = false;
			ConstrainMouse = true;
			StateManager.AddState(new TestState());
		}
	}
	public class TestState : State {
		VGame.Shapes.Cursor cursor = new VGame.Shapes.Cursor();
		VGame.Point playerPosition = new VGame.Point();
		public override void Update(GameTime gameTime) {
			playerPosition = InputManager.MousePosition;
			if (InputManager.MouseButtonState(MouseButton.Mouse0) == ButtonState.Pressed)
				Game.Exit();
		}
		public override void Draw(GameTime gameTime) {
			Context g = Renderer.Context;
			Renderer.Clear(ColorPresets.LightSteelBlue);
			Renderer.DrawText(new Vector2(Renderer.Width / 2, Renderer.Height / 2), "CLICK TO QUIT", 24, TextAlign.Center, TextAlign.Middle, new Color(1, 1, 1), new Color(0, 0, 0), null, 0, "pixel");
			cursor.Draw(Renderer, new Vector2(playerPosition.X, playerPosition.Y), 0, ColorPresets.White, ColorPresets.Black, 24);
		}
	}
	public class TestMenu : Menu {
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
	static class Program {
		static void Main(string[] args) {
			TestGame game = new TestGame();
			game.Run();
		}
	}
}
