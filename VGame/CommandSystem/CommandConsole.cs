using System;
using System.Collections.Generic;
using Cairo;

namespace VGame.CommandSystem {
	public class CommandConsole {
		public Vector2 Position = new Vector2();
		public int LinesToShow = 10;
		private CommandManager commandManager;
		public List<ConsoleMessage> History = new List<ConsoleMessage>();
		public string Buffer = "";
		private bool lastIsActive = false;
		public bool IsActive = false;
		public bool IsVisible = false;
		static readonly string cmdLineFormat = "> {0}";

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
			if (IsActive) {
				List<char> ascii = commandManager.Game.InputManager.GetTextInput();
				if (commandManager.Game.InputManager.KeyState(Keys.Backspace) == ButtonState.Pressed && Buffer.Length > 0) {
					Buffer = Buffer.Substring(0, Buffer.Length - 1);
				}
				if (commandManager.Game.InputManager.KeyState(Keys.Enter) == ButtonState.Pressed) {
					Buffer = Buffer.Trim();
					WriteLine(string.Format(cmdLineFormat, Buffer), ConsoleMessageType.Input);
					if (Buffer.Length > 0) {
						commandManager.Run(Buffer);
						Buffer = "";
					}
					//IsActive = false;
				}
				if (commandManager.Game.InputManager.KeyState(Keys.Escape) == ButtonState.Pressed)
					IsActive = false;
				if (lastIsActive)
					foreach (char c in ascii)
						// TODO: MAKE THIS RELEVANT TO THE BOUND CONSOLE KEY
						//if (c != '`' && c != '~')
							Buffer += c;
			}
			lastIsActive = IsActive;
		}

		public void Draw(GameTime gameTime) {
			if (!IsVisible)
				return;
			Context g = commandManager.Game.Renderer.Context;

			int linesToShow = Math.Min(LinesToShow, History.Count);
			Vector2 offset = Vector2.Zero;
			for (int i = 0; i < linesToShow; i++) {
				ConsoleMessage msg = History[History.Count - linesToShow + i];
				Color col = ColorPresets.White;
				if (msg.HasType(ConsoleMessageType.Input))
					col = ColorPresets.Gray75;
				if (msg.HasType(ConsoleMessageType.Warning))
					col = ColorPresets.Yellow;
				if (msg.HasType(ConsoleMessageType.Error))
					col = ColorPresets.Red;
				offset.Y += commandManager.Game.Renderer.DrawText(Position + offset, msg.ToString(), 12, TextAlign.Left, TextAlign.Top, col, null, new Cairo.Color(0, 0, 0, 0.6), 0, "console", 0).Height;
			}
			if (IsActive)
				commandManager.Game.Renderer.DrawText(Position + offset, string.Format(cmdLineFormat, Buffer), 12, TextAlign.Left, TextAlign.Top, ColorPresets.White, null, new Cairo.Color(0, 0, 0, 0.8), 0, "console", 0);
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

		public bool HasType(ConsoleMessageType type) {
			return (Type & type) == type;
		}

		public override string ToString() {
			string t = (Type & ConsoleMessageType.Client) == ConsoleMessageType.Client ? "[C] " : (Type & ConsoleMessageType.Server) == ConsoleMessageType.Server ? "[S] " : "";
			return string.Format("{0}{1}", t, Contents);
		}
	}

	[Flags]
	public enum ConsoleMessageType {
		Normal = 0,
		Client = 1,
		Server = 2,
		Error = 4,
		Warning = 8,
		Input = 16
	}
}

