using System;
using System.Linq;
using System.Collections.Generic;
using Cairo;

namespace VGame {
	public class StateManager {
		List<State> states = new List<State>();
		List<State> statesToUpdate = new List<State>();
		List<State> statesToDraw = new List<State>();

		public Game Game;
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
				return Game.Renderer;
			}
		}
		public InputManager InputManager {
			get {
				return Game.InputManager;
			}
		}

		public StateManager(Game game) {
			Game = game;
		}

		public void AddState(State state) {
			state.StateManager = this;
			states.Add(state);
			state.Initialize();
			FocusLastState();
		}

		public void RemoveState() {
			RemoveState(states.Last());
		}
		public void RemoveState(State state) {
			states.Remove(state);
			FocusLastState();
		}
		public void RemoveStateAt(int index) {
			states.RemoveAt(index);
			FocusLastState();
		}
		public void ReplaceState(State state) {
			if (states.Count > 0)
				states.Last().Exit();
			AddState(state);
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

		protected void FocusLastState() {
			if (states.Count > 0)
				states.Last().OnFocus();
		}

		public void Update(GameTime gameTime) {
			foreach (State state in states)
				statesToUpdate.Add(state);
			bool lastStateInputHandled = !Game.IsActive;
			while (statesToUpdate.Count > 0) {
				State state = statesToUpdate.Last();
				statesToUpdate.RemoveAt(statesToUpdate.Count - 1);
				state.Update(gameTime);
				if (!lastStateInputHandled) {
					if (!Game.SuppressInput)
						state.HandleInput(gameTime);
					lastStateInputHandled = true;
				}
			}
		}
		public void Draw(GameTime gameTime) {
			if (GameUpdateReady())
				return;
			foreach (State state in states)
				statesToDraw.Add(state);
			foreach (State state in statesToDraw) {
				if (GameUpdateReady())
					return;
				state.Draw(gameTime);
				if (GameUpdateReady())
					return;
			}
			statesToDraw.Clear();
		}
		private bool GameUpdateReady() {
			if (Game.ReadyToUpdate) {
				statesToDraw.Clear();
				return true;
			}
			return false;
		}
	}
}

