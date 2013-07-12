using System;
using System.Collections.Generic;
using Cairo;

namespace VGame {
	public class CommandConsole {
		public Vector2 Position = new Vector2();
		public int LinesToShow = 10;
		private CommandManager commandManager;
		public List<ConsoleMessage> History = new List<ConsoleMessage>();

		public CommandConsole(CommandManager commandManager) {
			this.commandManager = commandManager;
		}

		public void WriteLine(string str) {
			WriteLine(str, ConsoleMessageType.Normal);
		}
		public void WriteLine(string str, ConsoleMessageType type) {
			History.Add(new ConsoleMessage(str, type));
		}

		public void Draw(GameTime gameTime) {
			Context g = commandManager.Game.Renderer.Context;

			LinesToShow = Math.Min(LinesToShow, History.Count);
			Vector2 offset = Vector2.Zero;
			for (int i = 0; i < LinesToShow; i++) {
				offset.Y += commandManager.Game.Renderer.DrawText(Position + offset, History[History.Count - LinesToShow + i].ToString(), 12, TextAlign.Left, TextAlign.Top, ColorPresets.White, null, new Cairo.Color(0, 0, 0, 0.6), 0, "ProFontWindows", 0).Height;
			}
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

