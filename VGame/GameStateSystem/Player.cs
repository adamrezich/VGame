using System;

namespace VGame.GameStateSystem {
	public class Player {
		public string Name { get; internal set; }
		public int EntityID { get; internal set; }

		public Player(string name, int entityID) {
			Name = name;
			EntityID = entityID;
		}
	}

	public class PlayerEntity : Entity {
		public PlayerEntity() : base("player") {
		}
	}
}