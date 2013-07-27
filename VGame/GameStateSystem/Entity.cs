using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace VGame.GameStateSystem {
	public class Entity {

		// Properties
		public EntityClass Class { get; set; }
		public Dictionary<string, object> Properties { get; set; }

		// Constructor
		public Entity() {
			Properties = new Dictionary<string, object>();
		}

		// Public methods
		public void Serialize() {
		}
		public void Deserialize() {
		}
	}
}

