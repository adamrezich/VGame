using System;
using System.Collections.Generic;

namespace VGame.CommandSystem {
	public class CommandManager {
		public Game Game;
		public CommandConsole Console;
		public Dictionary<string, Variable> Variables = new Dictionary<string, Variable>();

		public CommandManager(Game game) {
			Game = game;
			Console = new CommandConsole(this);
			Commands.Load();
			VGame.CommandSystem.Variables.Load(this);
		}

		public void Run(string command) {
			Command cmd = null;
			try {
				cmd = Command.Parse(command);
			}
			catch (Exception e) {
				Console.WriteLine("ERROR: " + e.Message, ConsoleMessageType.Error);
				return;
			}
			Run(cmd);
		}
		public void Run(Command cmd) {
			try {
				cmd.Run(this);
			}
			catch (Exception e) {
				Console.WriteLine("ERROR: " + e.Message, ConsoleMessageType.Error);
			}
		}
	}
}

