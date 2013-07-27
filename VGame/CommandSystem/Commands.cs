using System;
using System.Collections.Generic;

using VGame.StateSystem;

namespace VGame.CommandSystem {
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
			Add("set", new CommandDefinition(new List<ParameterType>() { ParameterType.String, ParameterType.String }, delegate(CommandManager cmdMan, Command cmd) {
				//cmdMan.Console.WriteLine(cmdMan.Variables[cmd.Parameters[0].StringData].Value.ToString());
				if (!VariableDefinition.List.ContainsKey(cmd.Parameters[0].StringData))
					throw new Exception(string.Format("Variable \"{0}\" not found.", cmd.Parameters[0].StringData));
				Parameter param = new Parameter();
				VariableDefinition def = cmdMan.Variables[cmd.Parameters[0].StringData].Definition;
				switch (def.Type) {
					case ParameterType.Bool:
						bool b;
						if (bool.TryParse(cmd.Parameters[1].StringData, out b))
							param = new Parameter(b);
						else {
							int n;
							if (int.TryParse(cmd.Parameters[1].StringData, out n)) {
								if (n == 0 || n == 1)
									param = new Parameter(n == 1);
								else
									throw new Exception("Expected bool, got something else.");
							}
							else
								throw new Exception("Expected bool, got something else.");
						}
						break;
					case ParameterType.Int:
						int i;
						if (int.TryParse(cmd.Parameters[1].StringData, out i))
							param = new Parameter(i);
						else
							throw new Exception("Expected int, got something else.");
						break;
					case ParameterType.Float:
						float f;
						if (float.TryParse(cmd.Parameters[1].StringData, out f))
							param = new Parameter(f);
						else
							throw new Exception("Expected float, got something else.");
						break;
					case ParameterType.String:
						param = new Parameter(cmd.Parameters[1].StringData);
						break;
				}
				if (cmdMan.Game.IsClient() && !def.Flags.HasFlag(VariableFlags.Client)) {
					if (def.Flags.HasFlag(VariableFlags.Server) && !cmdMan.Game.IsServer())
						throw new Exception("You're not a server!");
					else if (!cmdMan.Game.IsServer())
						throw new Exception("Internal variable, access denied.");
				}
				if (cmdMan.Game.IsServer() && !def.Flags.HasFlag(VariableFlags.Server)) {
					if (def.Flags.HasFlag(VariableFlags.Client) && !cmdMan.Game.IsClient())
						throw new Exception("You're not a client!");
					else if (!cmdMan.Game.IsClient())
						throw new Exception("Internal variable, access denied.");
				}
				if (!cmdMan.Game.IsClient() && def.Flags.HasFlag(VariableFlags.Client))
					throw new Exception("You're not a client!");
				if (!cmdMan.Game.IsServer() && def.Flags.HasFlag(VariableFlags.Server))
					throw new Exception("You're not a server!");
				cmdMan.Variables[cmd.Parameters[0].StringData].Value = param;
			}));
		}
		public static void Add(string name, CommandDefinition commandDefinition) {
			CommandDefinition.Add(name, commandDefinition);
		}
	}
}

