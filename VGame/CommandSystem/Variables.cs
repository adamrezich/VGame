using System;
using System.Collections.Generic;

namespace VGame {
	public class Variables {
		public static void Load(CommandManager cmdMan) {
			Add(new VariableDefinition("cl_showfps", ParameterType.Bool, VariableFlags.Client));

			foreach (KeyValuePair<string, VariableDefinition> kvp in VariableDefinition.List) {
				cmdMan.Variables.Add(kvp.Key, new Variable(kvp.Value, kvp.Value.DefaultValue));
			}
		}
		public static void Add(VariableDefinition varDef) {
			VariableDefinition.List.Add(varDef.Name, varDef);
		}
	}
}

