using System;
using Cairo;

namespace VGame {
	public abstract class Shape {
		public abstract void Draw(Context g, Vector2 position, double direction, Color? fillColor, Color? strokeColor, double scale);
	}
}
namespace VGame.Shapes {
	public class Cursor : Shape {
		public override void Draw(Context g, Vector2 position, double direction, Color? fillColor, Color? strokeColor, double scale) {
			g.MoveTo(position.ToPointD());
			g.LineTo(position.AddLengthDir(scale, MathHelper.PiOver2).ToPointD());
			g.LineTo(position.AddLengthDir(scale, MathHelper.PiOver4 * 9).ToPointD());
			g.ClosePath();
			Util.StrokeAndFill(g, fillColor, strokeColor);
		}
	}
}

