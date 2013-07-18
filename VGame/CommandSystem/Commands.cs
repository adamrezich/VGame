using System;
using System.Collections.Generic;

namespace VGame {
	public class Commands {
		public static void Load() {
			Add("quit", new CommandDefinition(delegate(CommandManager cmdMan, Command cmd) {
				cmdMan.Game.Exit();
			}));
			Add("echo", new CommandDefinition(new List<ParameterType>() { ParameterType.String }, delegate(CommandManager cmdMan, Command cmd) {
				cmdMan.Console.WriteLine(cmd.Parameters[0].StringData);
			}));
			Add("escape", new CommandDefinition(delegate(CommandManager cmdMan, Command cmd) {
				State s = cmdMan.Game.StateManager.LastActiveState;
				if (s == null)
					return;
				s.OnEscape();
			}));
			Add("bind", new CommandDefinition(new List<ParameterType>() { ParameterType.String, ParameterType.String }, delegate(CommandManager cmdMan, Command cmd) {
				Keys key;
				if (Enum.TryParse<Keys>(cmd.Parameters[0].StringData, out key)) {
					Command test = null;
					string newCmd = cmd.Parameters[1].StringData.Replace("'", "\"");
					try {
						test = Command.Parse(newCmd);
					}
					catch (Exception e) {
						throw new Exception(string.Format("Would-be-bound command was invalid: {0}", e.Message));
					}
					if (test.Name == "bind")
						throw new Exception("Can't bind a key to a bind command.");
					else
						Binding.Bind(new KeyCombination(key, false, false, false), newCmd);
				}
				else
					throw new Exception(string.Format("Invalid key '{0}'.", cmd.Parameters[0].StringData));
			}));
			Add("clear", new CommandDefinition(delegate(CommandManager cmdMan, Command cmd) {
				cmdMan.Console.History.Clear();
			}));
			Add("menu_up", new CommandDefinition(typeof(Menu), delegate(CommandManager cmdMan, Command cmd) {
				if (!cmdMan.Game.InputManager.MouseMoved)
					((Menu)cmdMan.Game.StateManager.LastActiveState).OnMoveUp();
			}));
			Add("menu_down", new CommandDefinition(typeof(Menu), delegate(CommandManager cmdMan, Command cmd) {
				if (!cmdMan.Game.InputManager.MouseMoved)
					((Menu)cmdMan.Game.StateManager.LastActiveState).OnMoveDown();
			}));
			Add("menu_select", new CommandDefinition(typeof(Menu), delegate(CommandManager cmdMan, Command cmd) {
				if (!cmdMan.Game.InputManager.MouseMoved)
					((Menu)cmdMan.Game.StateManager.LastActiveState).OnSelect();
			}));
			Add("console_toggle", new CommandDefinition(delegate(CommandManager cmdMan, Command cmd) {
				cmdMan.Console.IsActive = !cmdMan.Console.IsActive;
				cmdMan.Console.IsVisible = cmdMan.Console.IsActive;
				if (!cmdMan.Console.IsActive)
					cmdMan.Console.Buffer = "";
			}));
			Add("get", new CommandDefinition(new List<ParameterType>() { ParameterType.String }, delegate(CommandManager cmdMan, Command cmd) {
				cmdMan.Console.WriteLine(cmdMan.Variables[cmd.Parameters[0].StringData].Value.ToString());
			}));
		}
		public static void Add(string name, CommandDefinition commandDefinition) {
			CommandDefinition.Add(name, commandDefinition);
		}
	}
}

