using System;
using System.Collections.Generic;

namespace VGame {
	public class KeyboardState {
		public Dictionary<Keys, ButtonState> Keys {
			get {
				return keys;
			}
		}
		Dictionary<Keys, ButtonState> keys;
		public KeyboardState() {
			this.keys = new Dictionary<VGame.Keys, ButtonState>();
		}
		public KeyboardState(Dictionary<Keys, ButtonState> keys) {
			this.keys = keys;
		}
	}
}

