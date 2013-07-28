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
			Entities.Add(id, new PlayerEntity());
			return id;
		}
		public void RemoveEntity(int id) {
			if (Entities.ContainsKey(id))
				Entities.Remove(id);
		}
		public void AddPlayer(int id) {

		}
		public void RemovePlayer(int id) {
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