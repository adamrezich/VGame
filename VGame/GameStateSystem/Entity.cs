using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Lidgren.Network;

namespace VGame.GameStateSystem {
	public abstract class Entity : IDisposable {

		// Properties
		public string Class {
			get {
				Type t = this.GetType();
				if (!reverseTypeList.ContainsKey(t))
					throw new Exception(string.Format("A programmer forgot to add a call to Entity.Add() for {0}, so it gets added to the entity database.", this.GetType().Name));
				return reverseTypeList[t];
			}
		}
		public string Name { get; internal set; }
		public short ID { get; internal set; }
		public abstract bool Shared { get; }
		internal bool JustCreated { get; set; }
		internal bool JustDestroyed { get; set; }

		// Static properties
		private static Dictionary<string, Type> typeList { get; set; }
		private static Dictionary<Type, string> reverseTypeList { get; set; }
		private static Dictionary<Type, PropertyInfo[]> propertyInfoList { get; set; }

		// Static fields
		private static Type[] allowedTypes = new Type[] { typeof(Int16), typeof(Int32), typeof(Int64), typeof(Single), typeof(Double), typeof(String), typeof(Boolean) };

		// Fields
		private Dictionary<string, object> properties = new Dictionary<string, object>();

		// Static constructor
		static Entity() {
			typeList = new Dictionary<string, Type>();
			reverseTypeList = new Dictionary<Type, string>();
			propertyInfoList = new Dictionary<Type, PropertyInfo[]>();
		}

		// Destructor
		~Entity() {
			CleanUpNativeResources();
		}

		// Static methods
		public static void Add(string name, Type type) {
			typeList.Add(name, type);
			reverseTypeList.Add(type, name);
			PropertyInfo[] props = type.GetProperties();
			List<PropertyInfo> propsToAdd = new List<PropertyInfo>();
			foreach (PropertyInfo prop in props) {
				if ((from row in prop.GetCustomAttributes(true) where row.GetType() == typeof(EntityPropertyAttribute) select row).FirstOrDefault() == null)
					continue;
				if (!allowedTypes.Any(supportedType => supportedType == prop.PropertyType))
					throw new Exception(string.Format("{0}'s \"{1}\" is a {2}, which is not an allowed type.", type.Name, prop.Name, prop.PropertyType));
				propsToAdd.Add(prop);
			}
			props = propsToAdd.ToArray();
			Array.Sort(props, delegate(PropertyInfo first, PropertyInfo second) {
				return first.MetadataToken.CompareTo(second.MetadataToken);
			});

			propertyInfoList.Add(type, props);
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
			if (!propertyInfoList.ContainsKey(this.GetType()))
				throw new Exception(string.Format("Class \"{0}\" not found in registered entity list!", this.GetType()));
			foreach (PropertyInfo prop in propertyInfoList[this.GetType()]) {
				var value = prop.GetValue(this, null);
				properties.Add(prop.Name, value);
			}
			return properties;
		}
		public void NetSerialize(ref NetOutgoingMessage msg) {
			msg.Write("__name");
			msg.Write(Name);
			msg.Write("__class");
			msg.Write(Class);
			foreach (PropertyInfo prop in propertyInfoList[this.GetType()]) {
				msg.Write(prop.Name);
				Type type = prop.PropertyType;
				if (type == typeof(Int16)) {
					msg.Write((short)prop.GetValue(this, null));
				}
				if (type == typeof(Int32)) {
					msg.Write((int)prop.GetValue(this, null));
				}
				if (type == typeof(Int64)) {
					msg.Write((long)prop.GetValue(this, null));
				}
				if (type == typeof(Single)) {
					msg.Write((float)prop.GetValue(this, null));
				}
				if (type == typeof(Double)) {
					msg.Write((double)prop.GetValue(this, null));
				}
				if (type == typeof(String)) {
					msg.Write((string)prop.GetValue(this, null));
				}
				if (type == typeof(Boolean)) {
					msg.Write((bool)prop.GetValue(this, null));
				}
			}
		}
		public void Dispose() {
			CleanUpManagedResources();
			CleanUpNativeResources();
			GC.SuppressFinalize(this);
		}

		// Protected methods
		protected virtual void CleanUpManagedResources() {
		}
		protected virtual void CleanUpNativeResources() {
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
			JustCreated = false;
			JustDestroyed = false;
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
			JustCreated = false;
			JustDestroyed = false;
		}

	}

	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class EntityPropertyAttribute : Attribute {
	}
}

