using System;
using System.Collections.Generic;
using Cairo;

namespace VGame {
	public class CommandConsole {
		private CommandManager commandManager;
		public List<string> History = new List<string>();

		public CommandConsole(CommandManager commandManager) {
			this.commandManager = commandManager;
		}

		public void WriteLine(string str) {
			History.Add(str);
		}

		public void Draw(GameTime gameTime, Vector2 position, int lines) {
			int height = 20;
			lines = Math.Min(lines, History.Count);
			for (int i = 0; i < lines; i++) {
				commandManager.Game.Renderer.DrawText(position + new Vector2(0, i * height), History[History.Count - lines + i], 12, TextAlign.Left, TextAlign.Top, ColorPresets.Black, null, ColorPresets.White, 0, "ProFontWindows");
			}
		}
	}
}

