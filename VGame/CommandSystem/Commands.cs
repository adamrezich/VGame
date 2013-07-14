using System;

namespace VGame {
	public class Commands {
		public static void Load() {
			Add("quit", typeof(Quit));
			Add("echo", typeof(Echo));
			Add("escape", typeof(Escape));
			Add("bind", typeof(Bind));
			Add("clear", typeof(Clear));
		}
		public static void Add(string name, Type type) {
			CommandDefinition.Add(name, type);
		}

		public class Quit : CommandDefinition {
			public Quit() : base() { }
			public override void Run(CommandManager cmdMan, Command cmd) {
				cmdMan.Game.Exit();
			}
		}
		public class Echo : CommandDefinition {
			public Echo() : base(ParameterType.String) { }
			public override void Run(CommandManager cmdMan, Command cmd) {
				cmdMan.Console.WriteLine(cmd.Parameters[0].StringData);
			}
		}
		public class Escape : CommandDefinition {
			public Escape() : base() { }
			public override void Run(CommandManager cmdMan, Command cmd) {
				State s = cmdMan.Game.StateManager.LastActiveState;
				if (s == null)
					return;
				s.OnEscape();
			}
		}
		public class Bind : CommandDefinition {
			public Bind() : base(ParameterType.String, ParameterType.String) { }
			public override void Run(CommandManager cmdMan, Command cmd) {
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
			}
		}
		public class Clear : CommandDefinition {
			public Clear() : base() { }
			public override void Run(CommandManager cmdMan, Command cmd) {
				cmdMan.Console.History.Clear();
			}
		}
	}
}

