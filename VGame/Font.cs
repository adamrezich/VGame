using System;
using Cairo;

namespace VGame {
	public struct Font : IDisposable {
		public ScaledFont ScaledFont;
		public bool Antialiased;
		public Font(ScaledFont scaledFont, bool antialiased) {
			ScaledFont = scaledFont;
			Antialiased = antialiased;
		}
		public void Dispose() {
			ScaledFont.Dispose();
		}
	}
}

