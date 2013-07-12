using System;
using System.Collections.Generic;
using System.Linq;

namespace VGame {
	public class Binding {
		public static List<Binding> List = new List<VGame.Binding>();

		public Keys Key;
		public string Command;

		private Binding(Keys key, string command) {
			Key = key;
			Command = command;
		}

		public static void BindKey(Keys key, string command) {
			if (List.Exists(x => x.Key == key && x.Command == command))
				return;
			List.Add(new Binding(key, command));
		}
		public static void UnbindKey(Keys key) {
			List.RemoveAll(x => x.Key == key);
		}
	}
}

