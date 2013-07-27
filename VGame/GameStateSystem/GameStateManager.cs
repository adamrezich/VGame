using System;
using System.Collections.Generic;
using System.Linq;

namespace VGame.GameStateSystem {
	public class GameStateManager {

		// Properties
		public Queue<GameState> States { get; internal set; }
		public int StatesToKeep { get; internal set; }
		public GameState CurrentGameState {
			get {
				if (States.Count == 0)
					return null;
				return States.Last();
			}
		}

		// Fields

		// Constructor
		public GameStateManager() {
			States = new Queue<GameState>();
			StatesToKeep = 10;
		}

		// Public methods
		public void Add(GameState state) {
			States.Enqueue(state);
			while (States.Count > StatesToKeep) {
				States.Dequeue();
			}
		}

	}
}

