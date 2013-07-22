using System;
using System.Collections.Generic;
using Cairo;

namespace VGame {
	public abstract class Shape {
		public AnimationType AnimationType;
		//public List<double> Tweens = new List<double>();
		public double AnimationSpeed = 0;
		public double AnimationProgress = 0;
		public bool LoopAnimation = true;
		public bool AnimationBackwards = false;

		public abstract void Draw(Renderer renderer, Vector2 position, double direction, Color? fillColor, Color? strokeColor, double scale);
		public void Update(GameTime gameTime) {
			switch (AnimationType) {
				case AnimationType.Linear:
					AnimationProgress = (double)MathHelper.Clamp((float)(AnimationProgress + AnimationSpeed), 0, 1);
					break;
				case AnimationType.Bounce:
					if (AnimationBackwards) {
						AnimationProgress = (double)MathHelper.Clamp((float)(AnimationProgress - AnimationSpeed), 0, 1);
						if (AnimationProgress <= 0)
							AnimationBackwards = false;
					}
					else {
						AnimationProgress = (double)MathHelper.Clamp((float)(AnimationProgress + AnimationSpeed), 0, 1);
						if (AnimationProgress >= 1)
							AnimationBackwards = true;
					}
					break;
			}
		}
	}
	public enum AnimationType {
		None,
		Linear,
		Bounce
	}
}
namespace VGame.Shapes {
	public class Cursor : Shape {
		public Cursor() : base() {
			AnimationType = AnimationType.Bounce;
			AnimationSpeed = 0;
		}
		public override void Draw(Renderer renderer, Vector2 position, double direction, Color? fillColor, Color? strokeColor, double scale) {
			Context g = renderer.Context;
			g.MoveTo(position.ToPointD());
			g.LineTo(position.AddLengthDir(scale, MathHelper.PiOver2 + (MathHelper.PiOver4 / 2 * AnimationProgress)).ToPointD());
			g.LineTo(position.AddLengthDir(scale, MathHelper.PiOver4 * 9 + (MathHelper.PiOver4 / 2 * AnimationProgress)).ToPointD());
			g.ClosePath();
			Util.StrokeAndFill(g, fillColor, strokeColor);
		}
	}
}

