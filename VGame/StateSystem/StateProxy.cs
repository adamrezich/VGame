using System;

namespace VGame {
	public class StateProxy : State {
		public State Now;
		public State After;
		bool updated = false;
		public StateProxy(State now, State after) {
			Now = now;
			After = after;
		}

		public override void Initialize() {
			StateManager.AddState(Now);
		}
		public override void Update(GameTime gameTime) {
			base.Update(gameTime);
			if (StateManager.LastActiveState != this)
				return;
			if (!updated) {
				updated = true;
				return;
			}
			StateManager.ReplaceState(After);
		}
	}
}

