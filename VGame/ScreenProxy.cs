using System;

namespace VGame {
	public class ScreenProxy : Screen {
		public Screen Now;
		public Screen After;
		bool updated = false;
		public ScreenProxy(Screen now, Screen after) {
			Now = now;
			After = after;
		}

		public override void Initialize() {
			ScreenManager.AddScreen(Now);
		}
		public override void Update() {
			base.Update();
			if (ScreenManager.LastActiveScreen != this)
				return;
			if (!updated) {
				updated = true;
				return;
			}
			ScreenManager.ReplaceScreen(After);
		}
	}
}

