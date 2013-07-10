using System;
using System.Collections.Generic;
using System.Linq;

namespace VGame {
	public abstract class Command {
		public string Name;
		public List<Parameter> Parameters = new List<Parameter>();
		public static Command Parse(string cmd) {
			List<string> split = cmd.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
			if (split.Count == 0)
				throw new Exception("Parse string empty.");
			string name = split[0];
			split.RemoveAt(0);
			if (CommandDefinition.List.ContainsKey(name)) {
				Console.WriteLine("found command!");
				CommandDefinition def = (CommandDefinition)Activator.CreateInstance(CommandDefinition.List[name]);
				//Command c = new Command<def>(new object[] { });
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
							else
								throw new Exception("Expected bool, got something else.");
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
				return (VGame.Command)Activator.CreateInstance(typeof(Command<>).MakeGenericType(CommandDefinition.List[name]), parameters.ToArray());
			}
			else
				throw new Exception("Command not found.");
		}
	}
	public class Command<TDefinition> : Command where TDefinition : CommandDefinition, new() {
		public TDefinition Definition;

		public Command(params object[] args) {
			TDefinition def = new TDefinition();
			Name = def.Name;

			for (int i = 0; i < args.Length; i++) {
				if (i >= def.Parameters.Count)
					throw new Exception("Too many arguments.");
				bool added = false;
				if (args[i] is bool && def.Parameters[i] == ParameterType.Bool) {
					Parameters.Add(new Parameter((bool)args[i]));
					added = true;
				}
				if (args[i] is int && def.Parameters[i] == ParameterType.Int) {
					Parameters.Add(new Parameter((int)args[i]));
					added = true;
				}
				if ((args[i] is float || args[i] is double) && def.Parameters[i] == ParameterType.Float) {
					Parameters.Add(new Parameter((float)args[i]));
					added = true;
				}
				if (args[i] is string && def.Parameters[i] == ParameterType.String) {
					Parameters.Add(new Parameter((string)args[i]));
					added = true;
				}
				if (!added)
					throw new Exception("Bad parameter type.");
			}
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
	}
	public abstract class CommandDefinition {
		public static Dictionary<string, Type> List = new Dictionary<string, Type>();
		public string Name;
		public List<ParameterType> Parameters;

		public CommandDefinition(string name, params ParameterType[] args) : this(name, args.ToList()) {
		}
		public CommandDefinition(string name, List<ParameterType> parameters) {
			Name = name;
			Parameters = parameters;
			if (!List.ContainsKey(Name))
				List.Add(Name, this.GetType());
		}
	}
	public struct Parameter {
		public ParameterType DataType {
			get {
				if (boolData.HasValue)
					return ParameterType.Bool;
				if (intData.HasValue)
					return ParameterType.Int;
				if (floatData.HasValue)
					return ParameterType.Float;
				if (stringData != null)
					return ParameterType.String;
				throw new Exception("Something went horribly wrong and a paramter lost it's data or something.");
			}
		}
		public bool BoolData {
			get {
				if (boolData.HasValue) {
					return (bool)boolData;
				}
				else
					throw new Exception("Trying to read bool data from parameter when there was none.");
			}
		}
		public int IntData {
			get {
				if (intData.HasValue)
					return (int)IntData;
				else
					throw new Exception("Trying to read int data from parameter when there was none.");
			}
		}
		public string StringData{
			get {
				return stringData;
			}
		}
		public float FloatData{
			get {
				if (floatData.HasValue)
					return (float)floatData;
				else
					throw new Exception("Trying to read float data from parameter when there was none.");
			}
		}
		bool? boolData;
		int? intData;
		float? floatData;
		string stringData;
		public Parameter(bool boolData) {
			this.boolData = boolData;
			this.intData = null;
			this.floatData = null;
			this.stringData = null;
		}
		public Parameter(int intData) {
			this.boolData = null;
			this.intData = intData;
			this.floatData = null;
			this.stringData = null;
		}
		public Parameter(float floatData) {
			this.boolData = null;
			this.intData = null;
			this.floatData = floatData;
			this.stringData = null;
		}
		public Parameter(string stringData) {
			this.boolData = null;
			this.intData = null;
			this.floatData = null;
			this.stringData = stringData;
		}
	}
	public enum ParameterType {
		Bool,
		Int,
		Float,
		String
	}
}

