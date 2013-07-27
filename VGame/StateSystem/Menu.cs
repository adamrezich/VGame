using System;
using System.Collections.Generic;
using Cairo;

namespace VGame.StateSystem {
	public class Menu : State {
		public string Title;
		public string Description;
		public bool Cancelable = true;
		public int Width = 400;
		public int Margin = 8;

		List<MenuEntry> entries = new List<MenuEntry>();
		int? selectedIndex = 0;
		bool mousing = false;
		Shapes.Cursor cursor = new VGame.Shapes.Cursor();
		protected IList<MenuEntry> Entries {
			get {
				return entries;
			}
		}

		public int Height {
			get {
				int height = 24 + Margin * 2;
				foreach (MenuEntry e in entries) {
					height += e.Height;
				}
				return height;
			}
		}

		public Menu(string title) {
			Title = title;
		}
		public Menu(string title, bool cancelable) {
			Title = title;
			Cancelable = cancelable;
		}

		public override void HandleInput(GameTime gameTime) {
			if (InputManager.MouseMoved)
				mousing = true;
			if (InputManager.MouseButtonState(MouseButton.Left) == ButtonState.Pressed) {
				UpdateSelected();
				if (selectedIndex.HasValue && entries[(int)selectedIndex].Enabled)
					OnSelectEntry((int)selectedIndex);
			}
			if (InputManager.KeyState(Keys.Left) == ButtonState.Pressed) {
				mousing = false;
				if (selectedIndex.HasValue && entries[(int)selectedIndex].Enabled)
					OnSwipeLeftEntry((int)selectedIndex);
			}
			if (InputManager.KeyState(Keys.Right) == ButtonState.Pressed) {
				mousing = false;
				if (selectedIndex.HasValue && entries[(int)selectedIndex].Enabled)
					OnSwipeRightEntry((int)selectedIndex);
			}
			List<char> unicode = InputManager.GetTextInput();
			if (unicode.Count > 0 || InputManager.KeyState(Keys.Backspace) == ButtonState.Pressed) {
				mousing = false;
				if (selectedIndex.HasValue && entries[(int)selectedIndex].Enabled)
					OnTextEntry((int)selectedIndex, unicode, InputManager.KeyState(Keys.Backspace) == ButtonState.Pressed);
			}
		}
		protected void UpdateSelected() {
			bool foundGoodOne = false;
			if (mousing)
				selectedIndex = null;
			foreach (MenuEntry e in entries) {
				if (selectedIndex == null && !mousing) {
					if (!foundGoodOne && e.Enabled) {
						foundGoodOne = true;
						selectedIndex = entries.IndexOf(e);
					}
				}
				e.IsSelected = false;
				if (mousing && e.Rectangle.Contains(InputManager.MousePosition)) {
					selectedIndex = entries.IndexOf(e);
				}
				if (selectedIndex == entries.IndexOf(e))
					e.IsSelected = true;
			}
		}

		public override void OnFocus() {
			Game.ConstrainMouse = false;
		}
		public override void Update(GameTime gameTime) {
			base.Update(gameTime);
			UpdateSelected();
			foreach (MenuEntry e in entries)
				e.Update();
		}
		public override void Draw(GameTime gameTime) {
			Cairo.Context g = Renderer.Context;

			Vector2 midTop = new Vector2(Renderer.Width / 2, 0);
			g.MoveTo((midTop + new Vector2(-(Width / 2), 0)).ToPointD());
			g.LineTo((midTop + new Vector2(Width / 2, 0)).ToPointD());
			g.LineTo((midTop + new Vector2(Width / 2, Renderer.Height)).ToPointD());
			g.LineTo((midTop + new Vector2(-(Width / 2), Renderer.Height)).ToPointD());
			g.ClosePath();
			Renderer.SetColor(ColorPresets.Gray50);
			g.Fill();

			Vector2 origin = new Vector2(Renderer.Width / 2, Renderer.Height / 2 - Height / 2);
			Vector2 offset = new Vector2();
			if (Title != null) {
				string titleFont = "wide";
				int titleTextSize = 16;
				int titleHeight = 24;
				/*Cairo.Color titleBackgroundColor = new Cairo.Color(0.25, 0.25, 0.25);
				g.MoveTo((origin + offset + new Vector2(-(Width / 2), -8)).ToPointD());
				g.LineTo((origin + offset + new Vector2(Width / 2, -8)).ToPointD());
				g.LineTo((origin + offset + new Vector2(Width / 2, titleHeight)).ToPointD());
				g.LineTo((origin + offset + new Vector2(-(Width / 2), titleHeight)).ToPointD());
				g.ClosePath();
				Util.StrokeAndFill(g, titleBackgroundColor, null);*/
				Renderer.DrawText(origin + offset, Title, titleTextSize, TextAlign.Center, TextAlign.Top, new Cairo.Color(0.75, 0.75, 0.75), new Cairo.Color(0, 0, 0), null, 0, titleFont);
				offset.Y += titleHeight;
			}
			offset.Y += Margin;
			foreach (MenuEntry e in entries) {
				e.Draw(origin + offset, Width, 1f);
				offset.Y += e.Height;
			}
			if (IsLastActiveState) {
				cursor.Draw(Renderer, new Vector2(InputManager.MousePosition.X, InputManager.MousePosition.Y), 0, ColorPresets.White, ColorPresets.Black, 24);
			}
		}

		protected virtual void OnSelectEntry(int entryIndex) {
			if (entries[entryIndex].IsCancel)
				OnCancel();
			else
				entries[entryIndex].OnSelectEntry();
		}
		protected virtual void OnSwipeLeftEntry(int entryIndex) {
			entries[entryIndex].OnSwipeLeftEntry();
		}
		protected virtual void OnSwipeRightEntry(int entryIndex) {
			entries[entryIndex].OnSwipeRightEntry();
		}
		protected virtual void OnTextEntry(int entryIndex, List<char> ascii, bool backspace) {
			entries[entryIndex].OnTextInputEntry(ascii, backspace);
		}
		protected virtual void OnCancel() {
			Exit();
		}
		protected void OnCancel(object sender, EventArgs e) {
			OnCancel();
		}

		public override void OnEscape() {
			if (Cancelable) {
				if (selectedIndex.HasValue && entries[(int)selectedIndex].IsCancel)
					OnCancel();
				else {
					foreach (MenuEntry e in entries) {
						if (e.IsCancel) {
							mousing = false;
							selectedIndex = entries.IndexOf(e);
							UpdateSelected();
						}
					}
				}
			}
		}
		public void OnMoveUp() {
			mousing = false;
			if (selectedIndex == null)
				selectedIndex = entries.Count;
			do {
				selectedIndex--;
				if (selectedIndex < 0)
					selectedIndex += entries.Count;
			} while (!entries[(int)selectedIndex].Enabled);
		}
		public void OnMoveDown() {
			mousing = false;
			if (selectedIndex == null)
				selectedIndex = -1;
			do {
				selectedIndex++;
				if (selectedIndex >= entries.Count)
					selectedIndex -= entries.Count;
			} while (!entries[(int)selectedIndex].Enabled);
		}
		public void OnSelect() {
			mousing = false;
			if (selectedIndex.HasValue && entries[(int)selectedIndex].Enabled)
				OnSelectEntry((int)selectedIndex);
		}
	}
}

