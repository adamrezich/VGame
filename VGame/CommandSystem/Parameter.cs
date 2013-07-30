using System;
using Lidgren.Network;

namespace VGame.CommandSystem {
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
				throw new Exception("Something went horribly wrong and a paramter lost its data or something.");
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
					return (int)intData;
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
		public Parameter(ParameterType type) {
			this.boolData = (type == ParameterType.Bool ? (bool?)false : (bool?)null);
			this.intData = (type == ParameterType.Int ? (int?)0 : (int?)null);
			this.floatData = (type == ParameterType.Float ? (float?)0 : (float?)null);
			this.stringData = (type == ParameterType.String ? "" : null);
		}
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

		public override string ToString() {
			switch (DataType) {
				case ParameterType.Bool:
					return boolData.ToString();
					case ParameterType.Int:
					return intData.ToString();
					case ParameterType.Float:
					return floatData.ToString();
					case ParameterType.String:
					return stringData;
			}
			throw new Exception("Somehow a variable didn't have a value.");
		}

		public void NetSerialize(ref NetOutgoingMessage msg) {
			switch (DataType) {
				case ParameterType.Bool:
					msg.Write((bool)boolData);
					break;
				case ParameterType.Int:
					msg.Write((int)intData);
					break;
				case ParameterType.Float:
					msg.Write((float)floatData);
					break;
				case ParameterType.String:
					msg.Write(stringData);
					break;
			}
		}
	}
	public enum ParameterType {
		Bool,
		Int,
		Float,
		String
	}
}

