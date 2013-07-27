using System;
using System.Collections.Generic;

namespace VGame.CommandSystem {
	public class Variable {
		public VariableDefinition Definition;
		public Parameter Value;
		public Variable(VariableDefinition definition, Parameter value) {
			Definition = definition;
			Value = value;
		}
	}

	public class VariableDefinition {
		public static Dictionary<string, VariableDefinition> List = new Dictionary<string, VariableDefinition>();

		public string Name { get; internal set; }
		public ParameterType Type { get; internal set; }
		public Parameter DefaultValue { get; internal set; }
		public VariableFlags Flags { get; internal set; }
		public string Description { get; internal set; }

		public VariableDefinition(string name, ParameterType type, VariableFlags flags) : this(name, type, flags, new Parameter(type), "") {
		}
		public VariableDefinition(string name, ParameterType type, VariableFlags flags, Parameter defaultValue) : this(name, type, flags, defaultValue, "") {
		}
		public VariableDefinition(string name, ParameterType type, VariableFlags flags, Parameter defaultValue, string description) {
			Name = name;
			Type = type;
			DefaultValue = defaultValue;
			Flags = flags;
			Description = description;
		}
	}

	[Flags]
	public enum VariableFlags {
		None = 0,
		Client = 1,
		Server = 2,
		Cheat = 4,
		Protected = 8,
		UserInfo = 16,
		Replicated = 32,
		Archive = 64,
		NotConnected = 128
	}
}

