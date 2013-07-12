using System;

namespace VGame {
	public class CommandManager {
		public Game Game;
		public CommandConsole Console;

		public CommandManager(Game game) {
			Game = game;
			Console = new CommandConsole(this);
			VGame.Commands.Load();
		}

		public void Run(string command) {
			Command cmd = null;
			try {
				cmd = Command.Parse(command);
			}
			catch (Exception e) {
				Console.WriteLine("ERROR PARSING COMMAND: " + e.Message);
				return;
			}
			Run(cmd);
		}
		public void Run(Command cmd) {
			try {
				cmd.Run(this);
			}
			catch (Exception e) {
				Console.WriteLine("ERROR RUNNING COMMAND: " + e.Message);
			}
		}
	}
}

