using System;
using Cairo;

namespace VGame {
	public abstract class State {
		public StateManager StateManager {
			get {
				return stateManager;
			}
			internal set {
				stateManager = value;
			}
		}
		StateManager stateManager;
		public InputManager InputManager {
			get {
				return stateManager.InputManager;
			}
		}
		public Renderer Renderer {
			get {
				return stateManager.Renderer;
			}
		}
		public Game Game {
			get {
				return stateManager.Game;
			}
		}
		public bool IsExiting {
			get {
				return isExiting;
			}
			protected internal set {
				isExiting = value;
			}
		}
		bool isExiting = false;
		public bool IsLastActiveState {
			get {
				return StateManager.LastActiveState == this;
			}
		}

		public State() {
		}

		public virtual void Initialize() {
		}
		public virtual void HandleInput(GameTime gameTime) {
		}
		public virtual void Draw(GameTime gameTime) {
		}
		public virtual void Update(GameTime gameTime) {
		}
		public virtual void OnFocus() {
		}
		public void Exit() {
			isExiting = true;
			StateManager.RemoveState(this);
		}
	}
}