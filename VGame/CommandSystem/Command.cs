using System;
using System.Collections.Generic;
using System.Linq;

namespace VGame.CommandSystem {
	public class Command {
		public string Name;
		public List<Parameter> Parameters = new List<Parameter>();
		public CommandDefinition CommandDefinition;

		public Command(CommandDefinition commandDefinition, params object[] args) {
			CommandDefinition = commandDefinition;
			Name = commandDefinition.Name;
			if (args.Length < commandDefinition.Parameters.Count) {
				throw new Exception("Too few arguments.");
			}
			if (args.Length > commandDefinition.Parameters.Count) {
				throw new Exception("Too many arguments.");
			}
			for (int i = 0; i < args.Length; i++) {
				bool added = false;
				if (args[i] is bool && commandDefinition.Parameters[i] == ParameterType.Bool) {
					Parameters.Add(new Parameter((bool)args[i]));
					added = true;
				}
				if (args[i] is int && commandDefinition.Parameters[i] == ParameterType.Int) {
					Parameters.Add(new Parameter((int)args[i]));
					added = true;
				}
				if ((args[i] is float || args[i] is double) && commandDefinition.Parameters[i] == ParameterType.Float) {
					Parameters.Add(new Parameter((float)args[i]));
					added = true;
				}
				if (args[i] is string && commandDefinition.Parameters[i] == ParameterType.String) {
					Parameters.Add(new Parameter((string)args[i]));
					added = true;
				}
				if (!added)
					throw new Exception("Bad parameter type.");
			}
		}

		public void Run(CommandManager commandManager) {
			CommandDefinition.Run(commandManager, this);
		}

		public override string ToString() {
			string str = Name;
			foreach (Parameter p in Parameters) {
				str += " ";
				switch (p.DataType) {
					case ParameterType.Bool:
						str += p.BoolData ? "1" : "0";
						break;
						case ParameterType.Int:
						str += p.IntData.ToString();
						break;
						case ParameterType.Float:
						str += p.FloatData.ToString();
						break;
						case ParameterType.String:
						str += p.StringData;
						break;
				}
			}
			return str;
		}
		
		public static Command Parse(string cmd) {
			cmd = cmd.Trim();
			if (cmd.Length == 0)
				throw new Exception("");
			List<string> split = cmd.Split('"').Select((element, index) => index % 2 == 0 ? element.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries) : new string[] { element }).SelectMany(element => element).ToList();
			string name = split[0];
			split.RemoveAt(0);
			if (CommandDefinition.List.ContainsKey(name)) {
				CommandDefinition def = CommandDefinition.List[name];
				if (split.Count != def.Parameters.Count) {
					throw new Exception("Incorrect parameter count.");
				}
				List<object> parameters = new List<object>();
				for (int i = 0; i < split.Count; i++) {
					switch (def.Parameters[i]) {
						case ParameterType.Bool:
							bool b;
							if (bool.TryParse(split[i], out b))
								parameters.Add(b);
							else {
								int n;
								if (int.TryParse(split[i], out n)) {
									if (n == 0 || n == 1)
										parameters.Add(n == 1);
									else
										throw new Exception("Expected bool, got something else.");
								}
								else
									throw new Exception("Expected bool, got something else.");
							}
							break;
						case ParameterType.Int:
							int _i;
							if (int.TryParse(split[i], out _i))
								parameters.Add(_i);
							else
								throw new Exception("Expected int, got something else.");
							break;
						case ParameterType.Float:
							float f;
							if (float.TryParse(split[i], out f))
								parameters.Add(f);
							else
								throw new Exception("Expected float, got something else.");
							break;
						case ParameterType.String:
							parameters.Add(split[i]);
							break;
					}
				}
				return (Command)Activator.CreateInstance(typeof(Command), def, parameters.ToArray());
			}
			else if (VariableDefinition.List.ContainsKey(name)) {
				if (split.Count == 0)
					return Parse("get " + cmd);
				else
					return Parse("set " + cmd);
			}
			else
				throw new Exception("Command not found.");
		}
	}
	public class CommandDefinition {
		public static Dictionary<string, CommandDefinition> List = new Dictionary<string, CommandDefinition>();
		public string Name {
			get {
				return List.FirstOrDefault(x => x.Value == this).Key;
			}
		}
		public List<ParameterType> Parameters;
		public Type State = null;
		public Action<CommandManager, Command> RunAction;
		public string Description { get; private set; }

		public CommandDefinition(Action<CommandManager, Command> runAction) : this(new List<ParameterType>(), typeof(StateSystem.State), runAction, "") {
		}
		public CommandDefinition(List<ParameterType> parameters, Action<CommandManager, Command> runAction) : this(parameters, typeof(StateSystem.State), runAction, "") {
		}
		public CommandDefinition(Type state, Action<CommandManager, Command> runAction) : this(new List<ParameterType>(), state, runAction, "") {
		}
		public CommandDefinition(List<ParameterType> parameters, Type state, Action<CommandManager, Command> runAction, string description) {
			Parameters = parameters;
			State = state;
			RunAction = runAction;
			Description = description;
		}

		public void Run(CommandManager cmdManager, Command cmd) {
			if (cmdManager.Game.StateManager == null || cmdManager.Game.StateManager.StateCount == 0) {
				if (State == typeof(StateSystem.State))
					RunAction.Invoke(cmdManager, cmd);
				return;
			}
			Type t = cmdManager.Game.StateManager.LastActiveState.GetType();
			if (t.IsSubclassOf(State) || t == State)
				RunAction.Invoke(cmdManager, cmd);
		}

		public static void Add(string name, CommandDefinition commandDefinition) {
			if (!List.ContainsKey(name))
				List.Add(name, commandDefinition);
			else
				throw new Exception("Attempted to override existing command");
		}

		public override string ToString() {
			return string.Format("{0} | {1}", Name, Description);
		}
	}
}

