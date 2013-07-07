using System;
using Cairo;

namespace VGame {
	public static class Util {
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

