using System;
using Cairo;

namespace VGame {
	public struct Font {
		public ScaledFont ScaledFont;
		public bool Antialiased;
		public Font(ScaledFont scaledFont, bool antialiased) {
			ScaledFont = scaledFont;
			Antialiased = antialiased;
		}
	}
}

