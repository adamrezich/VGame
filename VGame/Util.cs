using System;
using Cairo;

namespace VGame {
	public static class Util {
		public static void StrokeAndFill(Context g, Cairo.Color? fillColor, Cairo.Color? strokeColor) {
			if (fillColor.HasValue && fillColor != null) {
				g.SetSourceRGBA(((Cairo.Color)fillColor).R, ((Cairo.Color)fillColor).G, ((Cairo.Color)fillColor).B, ((Cairo.Color)fillColor).A);
				if (strokeColor.HasValue && fillColor != null)
					g.FillPreserve();
				else
					g.Fill();
			}
			if (strokeColor.HasValue && strokeColor != null) {
				g.SetSourceRGBA(((Cairo.Color)strokeColor).R, ((Cairo.Color)strokeColor).G, ((Cairo.Color)strokeColor).B, ((Cairo.Color)strokeColor).A);
				g.Stroke();
			}
		}
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

