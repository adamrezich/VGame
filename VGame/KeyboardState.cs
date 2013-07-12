using System;
using System.Collections.Generic;

namespace VGame {
	public class KeyboardState {
		public Dictionary<Keys, ButtonState> Keys {
			get {
				return keys;
			}
			internal set {
				keys = value;
			}
		}
		public List<char> Unicode {
			get {
				return unicode;
			}
			internal set {
				unicode = value;
			}
		}
		List<char> unicode;
		Dictionary<Keys, ButtonState> keys;
		public KeyboardState() {
			keys = new Dictionary<VGame.Keys, ButtonState>();
			unicode = new List<char>();
		}
		public KeyboardState(Dictionary<Keys, ButtonState> keys, List<char> unicode) {
			this.keys = keys;
			this.unicode = unicode;
		}
	}
}