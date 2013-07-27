using System;
using System.Collections.Generic;
using Lidgren.Network;

namespace VGame.GameStateSystem {
	public class GameState {

		// Properties
		public SortedDictionary<int, Entity> Entities { get; internal set; }
		public SortedDictionary<int, Player> Players { get; internal set; }

		// Constructor
		public GameState() {
			Entities = new SortedDictionary<int, Entity>();
			Players = new SortedDictionary<int, Player>();
		}

		// Public methods
		public int AddEntity() {
			int id = Entities.Count;
			Entities.Add(id, new Entity());
			return id;
		}

		// Static methods
		public static GameState GetDelta(GameState state1, GameState state2) {
			return null;
		}
	}
	public enum GameStateObject {
		Entity,
		Player
	}
}