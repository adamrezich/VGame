using System;
using System.Linq;
using System.Collections.Generic;
using Cairo;

namespace VGame {
	public class StateManager {
		List<State> states = new List<State>();
		List<State> statesToUpdate = new List<State>();
		Game game;
		public State LastActiveState {
			get {
				State state = null;
				foreach (State s in states)
					if (!s.IsExiting)
						state = s;
				return state;
			}
		}
		public State LastState {
			get {
				State state = null;
				if (states.Count > 0)
					state = states.Last();
				return state;
			}
		}
		public Renderer Renderer {
			get {
				return game.Renderer;
			}
		}
		public InputManager InputManager {
			get {
				return game.InputManager;
			}
		}

		public StateManager(Game game) {
			this.game = game;
		}

		public void AddState(State state) {
			state.StateManager = this;
			state.InputManager = game.InputManager;
			states.Add(state);
			state.Initialize();
		}

		public void RemoveState() {
			RemoveState(states.Last());
		}
		public void RemoveState(State state) {
			states.Remove(state);
		}
		public void RemoveStateAt(int index) {
			states.RemoveAt(index);
		}
		public void ReplaceState(State state) {
			if (states.Count > 0)
				states[states.Count].Exit();
		}
		public void ReplaceAllStates(State state) {
			ClearStates();
			AddState(state);
		}
		public void ReplaceStateProxy(State now, State after) {
			ReplaceState(new StateProxy(now, after));
		}
		public void ClearStates() {
			states.Clear();
		}

		public void Update() {
			foreach (State state in states)
				statesToUpdate.Add(state);
			bool lastStateInputHandled = !game.IsActive;
			while (statesToUpdate.Count > 0) {
				State state = statesToUpdate.Last();
				statesToUpdate.RemoveAt(statesToUpdate.Count - 1);
				state.Update();
				if (lastStateInputHandled) {
					state.HandleInput();
					lastStateInputHandled = true;
				}
			}
		}
		public void Draw(Context g) {
			foreach (State state in states) {
				state.Draw(Renderer, g);
			}
		}
	}
}

