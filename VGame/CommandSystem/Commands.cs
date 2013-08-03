using System;
using System.Collections.Generic;

using VGame.StateSystem;
using VGame.Multiplayer;

namespace VGame.CommandSystem {
	public class Commands {
		public static void Load() {
			Add("quit", new CommandDefinition(delegate(CommandManager cmdMan, Command cmd, RemoteClient sender) {
				cmdMan.Game.Exit();
			}));
			Add("echo", new CommandDefinition(new List<ParameterType>() { ParameterType.String }, delegate(CommandManager cmdMan, Command cmd, RemoteClient sender) {
				cmdMan.Game.DebugMessage(cmd.Parameters[0].StringData);
			}));
			Add("escape", new CommandDefinition(delegate(CommandManager cmdMan, Command cmd, RemoteClient sender) {
				State s = cmdMan.Game.StateManager.LastActiveState;
				if (s == null)
					return;
				s.OnEscape();
			}));
			Add("bind", new CommandDefinition(new List<ParameterType>() { ParameterType.String, ParameterType.String }, delegate(CommandManager cmdMan, Command cmd, RemoteClient sender) {
				try {
					InputCombination inputCombo = InputCombination.Parse(cmd.Parameters[0].StringData);
					Command test = null;
					string newCmd = cmd.Parameters[1].StringData.Replace("'", "\"");
					try {
						test = Command.Parse(newCmd, sender);
					}
					catch (Exception e) {
						throw new Exception(string.Format("Would-be-bound command was invalid: {0}", e.Message));
					}
					if (test.Name == "bind")
						throw new Exception("Can't bind an input to a \"bind\" command. That would be silly.");
					Binding.Bind(inputCombo, newCmd);
				}
				catch (Exception e) {
					throw new Exception(string.Format("Binding failed: {0}", e.Message));
				}
			}));
			Add("clear", new CommandDefinition(delegate(CommandManager cmdMan, Command cmd, RemoteClient sender) {
				cmdMan.Console.History.Clear();
			}));
			Add("menu_up", new CommandDefinition(typeof(Menu), delegate(CommandManager cmdMan, Command cmd, RemoteClient sender) {
				if (!cmdMan.Game.InputManager.MouseMoved)
					((Menu)cmdMan.Game.StateManager.LastActiveState).OnMoveUp();
			}));
			Add("menu_down", new CommandDefinition(typeof(Menu), delegate(CommandManager cmdMan, Command cmd, RemoteClient sender) {
				if (!cmdMan.Game.InputManager.MouseMoved)
					((Menu)cmdMan.Game.StateManager.LastActiveState).OnMoveDown();
			}));
			Add("menu_select", new CommandDefinition(typeof(Menu), delegate(CommandManager cmdMan, Command cmd, RemoteClient sender) {
				if (!cmdMan.Game.InputManager.MouseMoved)
					((Menu)cmdMan.Game.StateManager.LastActiveState).OnSelect();
			}));
			Add("console_toggle", new CommandDefinition(delegate(CommandManager cmdMan, Command cmd, RemoteClient sender) {
				cmdMan.Console.IsActive = !cmdMan.Console.IsActive;
				cmdMan.Console.IsVisible = cmdMan.Console.IsActive;
				if (!cmdMan.Console.IsActive)
					cmdMan.Console.Buffer = "";
			}));
			Add("get", new CommandDefinition(new List<ParameterType>() { ParameterType.String }, delegate(CommandManager cmdMan, Command cmd, RemoteClient sender) {
				cmdMan.Game.DebugMessage(cmdMan.Variables[cmd.Parameters[0].StringData].Value.ToString());
			}));
			Add("set", new CommandDefinition(new List<ParameterType>() { ParameterType.String, ParameterType.String }, delegate(CommandManager cmdMan, Command cmd, RemoteClient sender) {
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
				if (cmdMan.Game.IsClient() && def.Flags.HasFlag(VariableFlags.UserInfo) && VGame.Multiplayer.Client.Local != null && VGame.Multiplayer.Client.Local.IsConnected == true) {
					if (!cmdMan.UserInfoToSend.Contains(cmd.Parameters[0].StringData))
						cmdMan.UserInfoToSend.Add(cmd.Parameters[0].StringData);
				}
			}));
			Add("say", new CommandDefinition(new List<ParameterType>() { ParameterType.String }, delegate(CommandManager cmdMan, Command cmd, RemoteClient sender) {
				if (sender == null) {
					if (cmdMan.Game.IsSinglePlayer) {
						Server.Local.BroadcastMessage(new Message(Client.Local.Name, cmd.Parameters[0].StringData, (byte)0));
					}
					else {
						if (cmdMan.Game.IsClient()) {
							Client.Local.SendCommand(cmd);
						}
						if (cmdMan.Game.IsServer()) {
							Server.Local.BroadcastMessage(new Message(cmd.Parameters[0].StringData, MessageType.System));
						}
					}
				}
				else {
					Server.Local.BroadcastMessage(new Message(Server.Local.GetPlayer(sender.PlayerID).Name, cmd.Parameters[0].StringData, (byte)0));
				}
			}));
			Add("host_writeconfig", new CommandDefinition(delegate(CommandManager cmdMan, Command cmd, RemoteClient sender) {
				cmdMan.Save();
			}));
			Add("help", new CommandDefinition(new List<ParameterType>() { ParameterType.String }, delegate(CommandManager cmdMan, Command cmd, RemoteClient sender) {
				if (CommandDefinition.List.ContainsKey(cmd.Parameters[0].StringData)) {
					cmdMan.Game.DebugMessage(CommandDefinition.List[cmd.Parameters[0].StringData].ToString());
				}
				if (VariableDefinition.List.ContainsKey(cmd.Parameters[0].StringData)) {
					cmdMan.Game.DebugMessage(VariableDefinition.List[cmd.Parameters[0].StringData].ToString());
				}
			}));
			Add("connect", new CommandDefinition(new List<ParameterType>() { ParameterType.String }, delegate(CommandManager cmdMan, Command cmd, RemoteClient sender) {
				if (cmdMan.Game.IsClient()) {
					cmdMan.Variables["ip"].Value = new Parameter(cmd.Parameters[0].StringData);
					if (Client.Local.IsConnected) {
						cmdMan.Run("disconnect", sender);
					}
					cmdMan.Variables["ip"].Value = cmd.Parameters[0];
					Client.Local.Connect();
				}
			}));
			Add("disconnect", new CommandDefinition(delegate(CommandManager cmdMan, Command cmd, RemoteClient sender) {
				if (cmdMan.Game.IsClient()) {
					if (Client.Local.IsConnected) {
						Client.Local.Disconnect("User intentionally disconnected.");
					}
					else {
						cmdMan.Game.DebugMessage("Not currently connected to a server.");
					}
				}
			}));
			Add("retry", new CommandDefinition(delegate(CommandManager cmdMan, Command cmd, RemoteClient sender) {
				if (cmdMan.Game.IsClient()) {
					Client.Local.Connect();
				}
			}));
		}
		public static void Add(string name, CommandDefinition commandDefinition) {
			CommandDefinition.Add(name, commandDefinition);
		}
	}
}

