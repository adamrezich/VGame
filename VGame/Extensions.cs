using System;
using Cairo;

namespace VGame {
	public static class Extensions {
		public static PointD ToPointD(this Vector2 v) {
			return new PointD(v.X, v.Y);
		}

		public static Vector2 AddLengthDir(this Vector2 v, double length, double dir) {
			return v + new Vector2((float)(Math.Cos(dir) * length), (float)(Math.Sin(dir) * length));
		}

		public static double LerpAngle(this double from, double to, double step) {
			// Ensure that 0 <= angle < 2pi for both "from" and "to" 
			while (from < 0)
				from += MathHelper.TwoPi;
			while (from >= MathHelper.TwoPi)
				from -= MathHelper.TwoPi;

			while (to < 0)
				to += MathHelper.TwoPi;
			while (to >= MathHelper.TwoPi)
				to -= MathHelper.TwoPi;

			if (System.Math.Abs(from - to) < MathHelper.Pi) {
				// The simple case - a straight lerp will do. 
				return (double)MathHelper.Lerp((float)from, (float)to, (float)step);
			}

			// If we get here we have the more complex case. 
			// First, increment the lesser value to be greater. 
			if (from < to)
				from += MathHelper.TwoPi;
			else
				to += MathHelper.TwoPi;

			float retVal = MathHelper.Lerp((float)from, (float)to, (float)step);

			// Now ensure the return value is between 0 and 2pi 
			if (retVal >= MathHelper.TwoPi)
				retVal -= MathHelper.TwoPi;
			return retVal;
		}

		public static string MakeDecimal(this string str) {
			if (str.Length < 2 || str.Substring(str.Length - 2, 1) != ".") {
				str += ".0";
			}
			return str;
		}

		public static bool IntersectsLine(this VGame.Rectangle r, Vector2 p1, Vector2 p2) {
			return IntersectsLine(r, new Point((int)p1.X, (int)p1.Y), new Point((int)p2.X, (int)p2.Y));
		}
		public static bool IntersectsLine(this VGame.Rectangle r, Point p1, Point p2) {
			return LineIntersectsLine(p1, p2, new Point(r.X, r.Y), new Point(r.X + r.Width, r.Y)) ||
				LineIntersectsLine(p1, p2, new Point(r.X + r.Width, r.Y), new Point(r.X + r.Width, r.Y + r.Height)) ||
				LineIntersectsLine(p1, p2, new Point(r.X + r.Width, r.Y + r.Height), new Point(r.X, r.Y + r.Height)) ||
				LineIntersectsLine(p1, p2, new Point(r.X, r.Y + r.Height), new Point(r.X, r.Y)) ||
				(r.Contains(p1) && r.Contains(p2));
		}

		private static bool LineIntersectsLine(Point l1p1, Point l1p2, Point l2p1, Point l2p2) {
			float q = (l1p1.Y - l2p1.Y) * (l2p2.X - l2p1.X) - (l1p1.X - l2p1.X) * (l2p2.Y - l2p1.Y);
			float d = (l1p2.X - l1p1.X) * (l2p2.Y - l2p1.Y) - (l1p2.Y - l1p1.Y) * (l2p2.X - l2p1.X);

			if (d == 0) {
				return false;
			}

			float r = q / d;

			q = (l1p1.Y - l2p1.Y) * (l1p2.X - l1p1.X) - (l1p1.X - l2p1.X) * (l1p2.Y - l1p1.Y);
			float s = q / d;

			if (r < 0 || r > 1 || s < 0 || s > 1) {
				return false;
			}

			return true;
		}
	}
}

