using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace VGame.GameStateSystem {
	public abstract class Entity {

		// Properties
		public string Class { get; internal set; }

		// Fields
		private Dictionary<string, object> properties = new Dictionary<string, object>();

		// Constructor
		public Entity(string _class) {
			Class = _class;
		}

		// Public methods
		public Dictionary<string, object> Serialize() {
			properties.Clear();
			properties.Add("__class", Class);
			PropertyInfo[] props = this.GetType().GetProperties();
			foreach (PropertyInfo prop in props) {
				object[] attrs = prop.GetCustomAttributes(true);
				object attr = (from row in attrs where row.GetType() == typeof(EntityPropertyAttribute) select row).FirstOrDefault();
				if (attr == null)
					continue;
				EntityPropertyAttribute eprop = attr as EntityPropertyAttribute;
				var value = prop.GetValue(this, null);
				properties.Add(prop.Name, value);
				/*Console.WriteLine(prop.Name);
				Console.WriteLine(value);*/
			}
			return properties;
		}
		public void Deserialize(Dictionary<string, object> data) {
		}

		[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
		public class EntityPropertyAttribute : Attribute {

		}
	}
}

