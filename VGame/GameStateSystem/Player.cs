using System;

namespace VGame.GameStateSystem {
	public class Player {
		public int EntityID { get; internal set; }

		public Player(int entityID) {
			EntityID = entityID;
		}
	}
}