using System;
using System.Collections.Generic;
using System.Linq;

namespace VGame.GameStateSystem {
	public class GameStateManager {

		// Properties
		public List<GameState> States { get; internal set; }
		public int StatesToKeep { get; internal set; }
		public GameState CurrentGameState {
			get {
				if (States.Count == 0)
					return null;
				return States.Last();
			}
		}
		public GameState CurrentGameStateDelta {
			get {
				if (States.Count == 0)
					return null;
				if (States.Count == 1)
					return States.Last();
				return GameState.GetDelta(States[States.Count - 2], States.Last());
			}
		}

		// Constructor
		public GameStateManager() {
			States = new List<GameState>();
			StatesToKeep = 10;
			Add(new GameState());
		}

		// Static constructor
		static GameStateManager() {
			Entity.Add("ent_player", typeof(PlayerEntity));
		}

		// Public methods
		public void Add(GameState state) {
			States.Add(state);
			while (States.Count > StatesToKeep) {
				States.RemoveAt(0);
			}
		}
		public void Add() {
			if (States.Count == 0)
				throw new Exception("Tried to create a new GameState from an existing one, but there was no existing one.");
			Add(CurrentGameState.CreateNext());

		}
		public GameState GetDeltaToCurrent(int tick) {
			if (tick >= CurrentGameState.Tick)
				throw new Exception("Player is updating faster than the server. This probably shouldn't be happening.");
			if (tick > (int)CurrentGameState.Tick - States.Count) {
				int index = (int)CurrentGameState.Tick - tick;
				return GameState.GetDelta(States[index], States.Last());
			}
			//if (tick < (int)CurrentGameState.Tick - States.Count) {
				return null;
			//}
		}

	}
}

