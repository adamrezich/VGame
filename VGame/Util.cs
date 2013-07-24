using System;
using Cairo;

namespace VGame {
	public static class Util {
		public static Color MakeColor(int r, int g, int b, double a) {
			return new Color((double)r / 255, (double)g / 255, (double)b / 255, a);
		}
	}
	public enum TextAlign {
		Left,
		Center,
		Right,
		Top,
		Middle,
		Bottom
	}
}

