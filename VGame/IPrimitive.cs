using System;
using System.Collections.Generic;
using Cairo;

namespace VGame {
	public interface IPrimitive {
		void Draw(Renderer renderer, Vector2 offset, double scale);
	}

	public struct Polygon : IPrimitive {
		public Point A {
			get {
				return a;
			}
			set {
				a = value;
			}
		}
		public Point B {
			get {
				return b;
			}
			set {
				b = value;
			}
		}
		public Point C {
			get {
				return c;
			}
			set {
				c = value;
			}
		}
		public List<Line> Lines {
			get {
				return new List<Line>() { new Line(a , b), new Line(b, c), new Line(c, a) };
			}
		}

		Point a;
		Point b;
		Point c;
		
		public Polygon(Point a, Point b, Point c) {
			this.a = a;
			this.b = b;
			this.c = c;
		}
		public Polygon(int ax, int ay, int bx, int by, int cx, int cy) {
			this.a = new Point(ax, ay);
			this.b = new Point(bx, by);
			this.c = new Point(cx, cy);
		}
		
		public void Draw(Renderer renderer, Vector2 offset, double scale) {
			Context g = renderer.Context;
			g.LineWidth = 1;
			g.MoveTo(A.X * scale + offset.X, A.Y * scale + offset.Y);
			g.LineTo(B.X * scale + offset.X, B.Y * scale + offset.Y);
			g.LineTo(C.X * scale + offset.X, C.Y * scale + offset.Y);
			g.ClosePath();
			renderer.StrokeAndFill(new Color(0.5, 0.5, 0.5, 0.25), new Color(0.5, 0.5, 0.5, 0.5));
			g.LineWidth = 2;
		}
		public void RotateAbout(Point origin, double angle) {
			a.X = (int)Math.Round((Math.Cos(angle) * (a.X - origin.X) - Math.Sin(angle) * (a.Y - origin.Y) + origin.X));
			a.Y = (int)Math.Round((Math.Sin(angle) * (a.X - origin.X) + Math.Cos(angle) * (a.Y - origin.Y) + origin.Y));
			b.X = (int)Math.Round((Math.Cos(angle) * (b.X - origin.X) - Math.Sin(angle) * (b.Y - origin.Y) + origin.X));
			b.Y = (int)Math.Round((Math.Sin(angle) * (b.X - origin.X) + Math.Cos(angle) * (b.Y - origin.Y) + origin.Y));
			c.X = (int)Math.Round((Math.Cos(angle) * (c.X - origin.X) - Math.Sin(angle) * (c.Y - origin.Y) + origin.X));
			c.Y = (int)Math.Round((Math.Sin(angle) * (c.X - origin.X) + Math.Cos(angle) * (c.Y - origin.Y) + origin.Y));
		}
		public Polygon ScaleAndOffset(double scale, Vector2 offset) {
			return new Polygon((int)Math.Round(a.X * scale + offset.X), (int)Math.Round(a.Y * scale + offset.Y), (int)Math.Round(b.X * scale + offset.X), (int)Math.Round(b.Y * scale + offset.Y), (int)Math.Round(c.X * scale + offset.X), (int)Math.Round(c.Y * scale + offset.Y));
		}
		private float sign(VGame.Point p1, VGame.Point p2, VGame.Point p3) {
			return (p1.X - p3.X) * (p2.Y - p3.Y) - (p2.X - p3.X) * (p1.Y - p3.Y);
		}
		public bool Contains(Point point) {
			bool b1, b2, b3;
			b1 = sign(point, a, b) < 0.0f;
			b2 = sign(point, b, c) < 0.0f;
			b3 = sign(point, c, a) < 0.0f;

			return ((b1 == b2) && (b2 == b3));
		}
		public bool Contains(Rectangle r) {
			return Contains(new Point(r.X, r.Y)) && Contains(new Point(r.X + r.Width, r.Y)) && Contains(new Point(r.X + r.Width, r.Y + r.Height)) && Contains(new Point(r.X, r.Y + r.Height));
		}
	}
}

