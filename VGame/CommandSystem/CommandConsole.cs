using System;
using System.Collections.Generic;
using Cairo;

namespace VGame {
	public class CommandConsole {
		public Vector2 Position = new Vector2();
		public int LinesToShow = 10;
		private CommandManager commandManager;
		public List<ConsoleMessage> History = new List<ConsoleMessage>();
		public string Buffer = "";
		public bool IsActive = false;
		public bool IsVisible = true;

		public CommandConsole(CommandManager commandManager) {
			this.commandManager = commandManager;
		}

		public void WriteLine(string str) {
			WriteLine(str, ConsoleMessageType.Normal);
		}
		public void WriteLine(string str, ConsoleMessageType type) {
			History.Add(new ConsoleMessage(str, type));
		}

		public void HandleInput() {
			if (commandManager.Game.InputManager.KeyState(Keys.Backquote) == ButtonState.Pressed)
				IsActive = !IsActive;
			if (IsActive) {
				List<char> ascii = commandManager.Game.InputManager.GetTextInput();
				if (commandManager.Game.InputManager.KeyState(Keys.Backspace) == ButtonState.Pressed && Buffer.Length > 0) {
					Buffer = Buffer.Substring(0, Buffer.Length - 1);
				}
				if (commandManager.Game.InputManager.KeyState(Keys.Enter) == ButtonState.Pressed) {
					if (Buffer.Length > 0) {
						commandManager.Run(Buffer);
						Buffer = "";
					}
					IsActive = false;
				}
				if (commandManager.Game.InputManager.KeyState(Keys.Escape) == ButtonState.Pressed)
					IsActive = false;
				foreach (char c in ascii)
					if (c != '`')
						Buffer += c;
			}
		}

		public void Draw(GameTime gameTime) {
			if (!IsVisible)
				return;
			Context g = commandManager.Game.Renderer.Context;

			int linesToShow = Math.Min(LinesToShow, History.Count);
			Vector2 offset = Vector2.Zero;
			for (int i = 0; i < linesToShow; i++) {
				offset.Y += commandManager.Game.Renderer.DrawText(Position + offset, History[History.Count - linesToShow + i].ToString(), 12, TextAlign.Left, TextAlign.Top, ColorPresets.White, null, new Cairo.Color(0, 0, 0, 0.6), 0, "ProFontWindows", 0).Height;
			}
			if (IsActive)
				commandManager.Game.Renderer.DrawText(Position + offset, string.Format("> {0}", Buffer), 12, TextAlign.Left, TextAlign.Top, ColorPresets.White, null, new Cairo.Color(0, 0, 0, 0.8), 0, "ProFontWindows", 0);
		}
	}

	public struct ConsoleMessage {
		public string Contents;
		public ConsoleMessageType Type;
		public ConsoleMessage(string contents) : this(contents, ConsoleMessageType.Normal) {
		}
		public ConsoleMessage(string contents, ConsoleMessageType type) {
			Contents = contents;
			Type = type;
		}
		public override string ToString() {
			string t = (Type & ConsoleMessageType.Client) == ConsoleMessageType.Client ? "[C] " : (Type & ConsoleMessageType.Server) == ConsoleMessageType.Server ? "[S] " : "";
			return string.Format("{0}{1}", t, Contents);
		}
	}

	[Flags]
	public enum ConsoleMessageType {
		Normal = 0x0,
		Client = 0x1,
		Server = 0x2,
		Error = 0x4
	}
}

