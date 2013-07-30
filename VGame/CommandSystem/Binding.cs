using System;
using System.Collections.Generic;
using System.Linq;

namespace VGame.CommandSystem {
	public class Binding {
		public static List<Binding> List = new List<Binding>();

		public InputCombination Combination;
		public string Command;

		private Binding(InputCombination combination, string command) {
			Combination = combination;
			Command = command;
		}

		public override string ToString() {
			return string.Format("bind {0} {1}", Combination.ToString(), Command.Contains(" ") ? string.Format("\"{0}\"", Command.Replace('"', '\'')) : Command);
		}

		public static void Bind(InputCombination combination, string command) {
			if (List.Exists(x => x.Combination == combination && x.Command == command))
				return;
			List.Add(new Binding(combination, command));
		}
		public static void Unbind(InputCombination combination) {
			List.RemoveAll(x => x.Combination == combination);
		}
	}
}

