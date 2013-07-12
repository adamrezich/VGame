using System;

namespace VGame {
	public class Commands {
		public static void Load() {
			Add("quit", typeof(Quit));
			Add("echo", typeof(Echo));
			Add("bind", typeof(Bind));
			//Add("conso
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
		public class Bind : CommandDefinition {
			public Bind() : base(ParameterType.String, ParameterType.String) { }
			public override void Run(CommandManager cmdMan, Command cmd) {
				Keys key;
				if (Enum.TryParse<Keys>(cmd.Parameters[0].StringData, out key)) {
					Command test = null;
					try {
						test = Command.Parse(cmd.Parameters[1].StringData);
					}
					catch (Exception e) {
						throw new Exception(string.Format("Would-be-bound command was invalid: {0}", e.Message));
					}
					if (test.Name == "bind")
						throw new Exception("Can't bind a key to a bind command.");
					else
						Binding.BindKey(key, cmd.Parameters[1].StringData);
				}
				else
					throw new Exception(string.Format("Invalid key '{0}'.", cmd.Parameters[0].StringData));
			}
		}
	}
}

