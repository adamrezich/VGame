using System;
using System.Collections.Generic;

namespace VGame {
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
		public string Name;
		public ParameterType Type;
		public Parameter DefaultValue;
		public VariableFlags Flags;

		public VariableDefinition(string name, ParameterType type, VariableFlags flags) : this(name, type, flags, new Parameter(type)) {
		}
		public VariableDefinition(string name, ParameterType type, VariableFlags flags, Parameter defaultValue) {
			Name = name;
			Type = type;
			DefaultValue = defaultValue;
			Flags = flags;
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

