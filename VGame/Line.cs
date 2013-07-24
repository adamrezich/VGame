using System;

namespace VGame {
	public struct Line {
		public Vector2 Point1;
		public Vector2 Point2;
		public Line(Point point1, Point point2) {
			Point1 = new Vector2(point1.X, point1.Y);
			Point2 = new Vector2(point2.X, point2.Y);
		}
		public Line(Vector2 point1, Vector2 point2) {
			Point1 = point1;
			Point2 = point2;
		}
		public Line(double x1, double y1, double x2, double y2) {
			Point1 = new Vector2((float)x1, (float)y1);
			Point2 = new Vector2((float)x2, (float)y2);
		}
	}
}

