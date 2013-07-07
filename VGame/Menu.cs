using System;
using System.Collections.Generic;
using Cairo;

namespace VGame {
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

		public Menu(string title) {
			Title = title;
		}
		public Menu(string title, bool cancelable) {
			Title = title;
			Cancelable = cancelable;
		}

		public override void HandleInput() {
			if (InputManager.MousePosition != InputManager.MousePositionLast)
				mousing = true;
		}
	}

	public class MenuEntry {
	}
}

