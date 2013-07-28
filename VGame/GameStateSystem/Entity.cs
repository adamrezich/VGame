using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace VGame.GameStateSystem {
	public abstract class Entity {

		// Properties
		public string Class {
			get {
				Type t = this.GetType();
				if (!reverseTypeList.ContainsKey(t))
					throw new Exception("A programmer forgot to add the entity to the entity list using a static constructor.");
				return reverseTypeList[t];
			}
		}
		public string Name { get; internal set; }
		public short ID { get; internal set; }
		public abstract bool Shared { get; }

		// Static properties
		private static Dictionary<string, Type> typeList { get; set; }
		private static Dictionary<Type, string> reverseTypeList { get; set; }

		// Fields
		private Dictionary<string, object> properties = new Dictionary<string, object>();

		// Static constructor
		static Entity() {
			typeList = new Dictionary<string, Type>();
			reverseTypeList = new Dictionary<Type, string>();
		}

		// Static methods
		public static void Add(string name, Type type) {
			typeList.Add(name, type);
			reverseTypeList.Add(type, name);
		}
		public static object Deserialize(Dictionary<string, object> data) {
			if (!data.ContainsKey("__class"))
				throw new Exception("__class entity parameter missing.");
			if (!typeList.ContainsKey((string)data["__class"]))
				throw new Exception(string.Format("Class \"{0}\" not found in registered entity list!", data["__class"]));
			Type type = typeList[(string)data["__class"]];
			object o = Activator.CreateInstance(type);
			foreach (KeyValuePair<string, object> kvp in data) {
				if (kvp.Key.StartsWith("__"))
					continue;
				var prop = type.GetProperty(kvp.Key);
				prop.SetValue(o, kvp.Value, null);
			}
			return o;
		}

		// Public methods
		public Dictionary<string, object> Serialize() {
			properties.Clear();
			properties.Add("__name", Name);
			properties.Add("__class", Class);
			PropertyInfo[] props = this.GetType().GetProperties();
			foreach (PropertyInfo prop in props) {
				object[] attrs = prop.GetCustomAttributes(true);
				object attr = (from row in attrs where row.GetType() == typeof(EntityPropertyAttribute) select row).FirstOrDefault();
				if (attr == null)
					continue;
				//EntityPropertyAttribute eprop = attr as EntityPropertyAttribute;
				var value = prop.GetValue(this, null);
				properties.Add(prop.Name, value);
			}
			return properties;
		}
	}

	public abstract class LocalEntity : Entity {

		// Properties
		public override bool Shared {
			get {
				return false;
			}
		}

		// Constructor
		public LocalEntity() : this("") {
		}
		public LocalEntity(string name) {
			Name = name;
		}

	}

	public abstract class SharedEntity : Entity {

		// Properties
		public override bool Shared {
			get {
				return true;
			}
		}

		// Constructor
		public SharedEntity() : this("") {
		}
		public SharedEntity(string name) {
			Name = name;
		}

	}

	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class EntityPropertyAttribute : Attribute {
	}

	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public class EntityFieldAttribute : Attribute {
	}
}

