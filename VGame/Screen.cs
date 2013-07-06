using System;
using Cairo;

namespace VGame {
	public abstract class Screen {
		public ScreenManager ScreenManager {
			get {
				return screenManager;
			}
			internal set {
				screenManager = value;
			}
		}
		ScreenManager screenManager;
		public bool IsExiting {
			get {
				return isExiting;
			}
			protected internal set {
				isExiting = value;
			}
		}
		bool isExiting = false;
		public bool IsLastActiveScreen {
			get {
				return ScreenManager.LastActiveScreen == this;
			}
		}

		public Screen() {
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
			ScreenManager.RemoveScreen(this);
		}
	}
}