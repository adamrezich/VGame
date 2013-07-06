using System;
using System.Linq;
using System.Collections.Generic;
using Cairo;

namespace VGame {
	public class ScreenManager {
		List<Screen> screens = new List<Screen>();
		List<Screen> screensToUpdate = new List<Screen>();
		Game game;
		public Screen LastActiveScreen {
			get {
				Screen screen = null;
				foreach (Screen s in screens)
					if (!s.IsExiting)
						screen = s;
				return screen;
			}
		}
		public Screen LastScreen {
			get {
				Screen screen = null;
				if (screens.Count > 0)
					screen = screens.Last();
				return screen;
			}
		}

		public ScreenManager(Game game) {
			this.game = game;
		}

		public void AddScreen(Screen screen) {
			screen.ScreenManager = this;
			screens.Add(screen);
			screen.Initialize();
		}

		public void RemoveScreen() {
			RemoveScreen(screens.Last());
		}
		public void RemoveScreen(Screen screen) {
			screens.Remove(screen);
		}
		public void RemoveScreenAt(int index) {
			screens.RemoveAt(index);
		}
		public void ReplaceScreen(Screen screen) {
			if (screens.Count > 0)
				screens[screens.Count].Exit();
		}
		public void ReplaceAllScreens(Screen screen) {
			ClearScreens();
			AddScreen(screen);
		}
		public void ReplaceScreenProxy(Screen now, Screen after) {
			ReplaceScreen(new ScreenProxy(now, after));
		}
		public void ClearScreens() {
			screens.Clear();
		}

		public void Draw(Context g) {
			foreach (Screen screen in screens) {
				screen.Draw(g);
			}
		}
	}
}

