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
		public virtual void HandleInput() {
		}
		public virtual void Draw(Context g) {
		}
		public virtual void Update() {
		}
		public void Exit() {
			isExiting = true;
			StateManager.RemoveState(this);
		}
	}
}