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
		public GameState CurrentGameStateDelta {
			get {
				if (States.Count == 0)
					return null;
				if (States.Count == 1)
					return States.Last();
				return GameState.GetDelta(lastState, States.LastOrDefault());
			}
		}

		// Fields
		private uint tick = 0;
		private GameState lastState = null;

		// Constructor
		public GameStateManager() {
			States = new Queue<GameState>();
			StatesToKeep = 10;
			Add(new GameState());
		}

		// Static constructor
		static GameStateManager() {
			Entity.Add("ent_player", typeof(PlayerEntity));
		}

		// Public methods
		public void Add(GameState state) {
			state.Tick = tick;
			tick++;
			if (States.Count > 0)
				lastState = States.Peek();
			States.Enqueue(state);
			while (States.Count > StatesToKeep) {
				States.Dequeue();
			}
		}

	}
}

