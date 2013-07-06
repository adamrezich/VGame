using System;
using Cairo;

namespace VGame {
	public static class Util {
		public static int TextBoxPadding = 4;
		public static Cairo.Color MakeColor(int r, int g, int b, double a) {
			return new Cairo.Color((double)b / 256, (double)g / 256, (double)r / 256, a);
		}
		public static void StrokeAndFill(Context g, Cairo.Color? fillColor, Cairo.Color? strokeColor) {
			if (fillColor.HasValue && fillColor != null) {
				g.Color = (Cairo.Color)fillColor;
				if (strokeColor.HasValue && fillColor != null)
					g.FillPreserve();
				else
					g.Fill();
			}
			if (strokeColor.HasValue && strokeColor != null) {
				g.Color = (Cairo.Color)strokeColor;
				g.Stroke();
			}
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

