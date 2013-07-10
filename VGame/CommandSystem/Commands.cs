using System;

namespace VGame {
	public class Commands {
		public static void Load() {
			CommandDefinition.Add("quit", typeof(Quit));
		}

		public class Quit : CommandDefinition {
			public Quit() : base() { }
			public override void Run(CommandManager cmdMan, Command cmd) {
				cmdMan.Game.Exit();
			}
		}
	}
}

