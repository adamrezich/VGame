using System;
using System.Collections.Generic;
using System.IO;

namespace VGame.CommandSystem {
	public class CommandManager {
		public Game Game;
		public CommandConsole Console;
		public Dictionary<string, Variable> Variables = new Dictionary<string, Variable>();
		public List<string> UserInfoToSend = new List<string>();

		public CommandManager(Game game) {
			Game = game;
			Console = new CommandConsole(this);
			Commands.Load();
			VGame.CommandSystem.Variables.Load(this);
			Load();
		}

		public void Run(string command) {
			Command cmd = null;
			try {
				cmd = Command.Parse(command);
			}
			catch (Exception e) {
				if (e.Message != "")
					Game.ErrorMessage(e.Message);
				return;
			}
			Run(cmd);
		}
		public void Run(Command cmd) {
			try {
				cmd.Run(this);
			}
			catch (Exception e) {
				Game.ErrorMessage(e.Message);
			}
		}

		public void Save() {
			using (StreamWriter file = new StreamWriter("config.cfg")) {
				foreach (KeyValuePair<string, Variable> kvp in Variables) {
					if (kvp.Value.Definition.Flags.HasFlag(VariableFlags.Archive)) {
						file.WriteLine(kvp.Value.ToString());
					}
				}
				foreach (Binding binding in Binding.List) {
					file.WriteLine(binding.ToString());
				}
			}
		}
		public void Load() {
			if (File.Exists("config.cfg")) {
				string[] lines = File.ReadAllLines("config.cfg");
				foreach (string line in lines) {
					Run(line);
				}
			}
			else {
				Game.DebugMessage("No config file found");
				LoadDefaults();
			}
		}
		public void LoadDefaults() {
			Binding.Bind(InputCombination.Create(Keys.Escape, false, false, false), "escape");
			Binding.Bind(InputCombination.Create(Keys.Escape, true, false, true), "quit");
			Binding.Bind(InputCombination.Create(Keys.Up, false, false, false), "menu_up");
			Binding.Bind(InputCombination.Create(Keys.Down, false, false, false), "menu_down");
			Binding.Bind(InputCombination.Create(Keys.Tab, false, false, false), "menu_down");
			Binding.Bind(InputCombination.Create(Keys.Tab, false, true, false), "menu_up");
			Binding.Bind(InputCombination.Create(Keys.Space, false, false, false), "menu_select");
			Binding.Bind(InputCombination.Create(Keys.Enter, false, false, false), "menu_select");
			Binding.Bind(InputCombination.Create(Keys.Backquote, false, false, false), "console_toggle");
		}
	}
}

