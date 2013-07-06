using System;

namespace VGame {
	public class Point {
		private int x;
		private int y;
		public int X {
			get {
				return x;
			}
			set {
				x = value;
			}
		}
		public int Y {
			get {
				return y;
			}
			set {
				y = value;
			}
		}

		public Point() {
			x = 0;
			y = 0;
		}
		public Point(int x, int y) {
			this.x = x;
			this.y = y;
		}
		public Point(Point point) {
			if ((object)point != null) {
				x = point.X;
				y = point.Y;
			}
			else
				throw new Exception("Tried to copy a null point!");
		}
	}
}

