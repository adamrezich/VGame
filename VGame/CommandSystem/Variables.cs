using System;
using System.Collections.Generic;

namespace VGame.CommandSystem {
	public class Variables {
		public static void Load(CommandManager cmdMan) {
			Add(new VariableDefinition("cl_showfps", ParameterType.Bool, VariableFlags.Client | VariableFlags.Archive, new Parameter(true), "Draw FPS at top of screen"));
			Add(new VariableDefinition("sv_cheats", ParameterType.Bool, VariableFlags.Server));
			Add(new VariableDefinition("sv_minrate", ParameterType.Int, VariableFlags.Server | VariableFlags.Archive, new Parameter(0), "Minimum client rate (0 means no limit)"));
			Add(new VariableDefinition("sv_maxrate", ParameterType.Int, VariableFlags.Server | VariableFlags.Archive, new Parameter(0), "Maximum client rate (0 means no limit)"));
			Add(new VariableDefinition("sv_minupdaterate", ParameterType.Int, VariableFlags.Server | VariableFlags.Archive, new Parameter(10), "Minimum client update rate"));
			Add(new VariableDefinition("sv_maxupdaterate", ParameterType.Int, VariableFlags.Server | VariableFlags.Archive, new Parameter(60), "Maximum client update rate"));
			Add(new VariableDefinition("cl_cmdrate", ParameterType.Int, VariableFlags.Client | VariableFlags.Archive | VariableFlags.UserInfo, new Parameter(30), "Max number of command packets sent to server per second"));
			Add(new VariableDefinition("cl_updaterate", ParameterType.Int, VariableFlags.Client | VariableFlags.Archive | VariableFlags.UserInfo, new Parameter(30), "Number of packets per second of updates you are requesting from the server"));
			Add(new VariableDefinition("rate", ParameterType.Int, VariableFlags.Archive | VariableFlags.UserInfo, new Parameter(12000), "Max bytes/sec the host can receive data"));

			foreach (KeyValuePair<string, VariableDefinition> kvp in VariableDefinition.List) {
				cmdMan.Variables.Add(kvp.Key, new Variable(kvp.Value, kvp.Value.DefaultValue));
			}
		}
		public static void Add(VariableDefinition varDef) {
			VariableDefinition.List.Add(varDef.Name, varDef);
		}
	}
}

