using System;

namespace VGame {
	public struct MouseState {
		int x, y;
		Point position;
		ButtonState scrollWheelUp;
		ButtonState scrollWheelDown;
		ButtonState leftButton;
		ButtonState rightButton;
		ButtonState middleButton;
		ButtonState xButton1;
		ButtonState xButton2;

		public MouseState(int x, int y, ButtonState scrollWheelUp, ButtonState scrollWheelDown, ButtonState leftButton, ButtonState middleButton, ButtonState rightButton, ButtonState xButton1, ButtonState xButton2) {
			this.x = x;
			this.y = y;
			position = new Point(x, y);
			this.scrollWheelUp = scrollWheelUp;
			this.scrollWheelDown = scrollWheelDown;
			this.leftButton = leftButton;
			this.middleButton = middleButton;
			this.rightButton = rightButton;
			this.xButton1 = xButton1;
			this.xButton2 = xButton2;
		}

		private void UpdatePosition() {
			position.X = x;
			position.Y = y;
		}

		public static bool operator ==(MouseState left, MouseState right) {
			return left.x == right.x &&
				left.y == right.y &&
				left.scrollWheelUp == right.scrollWheelUp &&
				left.scrollWheelDown == right.scrollWheelDown &&
				left.leftButton == right.leftButton &&
				left.middleButton == right.middleButton &&
				left.rightButton == right.rightButton &&
				left.xButton1 == right.xButton1 &&
				left.xButton2 == right.xButton2;
		}
		public static bool operator !=(MouseState left, MouseState right) {
			return !(left == right);
		}
		public override bool Equals(object obj) {
			if (obj is MouseState)
				return this == (MouseState)obj;
			return false;
		}
		public override int GetHashCode() {
			return base.GetHashCode();
		}

		public int X {
			get {
				return x;
			}
			internal set {
				x = value;
				UpdatePosition();
			}
		}
		public int Y {
			get {
				return y;
			}
			internal set {
				y = value;
				UpdatePosition();
			}
		}
		public Point Position {
			get {
				UpdatePosition();
				return position;
			}
			set {
				x = value.X;
				y = value.Y;
				UpdatePosition();
			}
		}
		public ButtonState ScrollWheelUp {
			get {
				return scrollWheelUp;
			}
			internal set {
				scrollWheelUp = value;
			}
		}
		public ButtonState ScrollWheelDown {
			get {
				return scrollWheelDown;
			}
			internal set {
				scrollWheelDown = value;
			}
		}
		public ButtonState LeftButton {
			get {
				return leftButton;
			}
			internal set {
				leftButton = value;
			}
		}
		public ButtonState MiddleButton {
			get {
				return middleButton;
			}
			internal set {
				middleButton = value;
			}
		}
		public ButtonState RightButton {
			get {
				return rightButton;
			}
			internal set {
				rightButton = value;
			}
		}
		public ButtonState XButton1 {
			get {
				return xButton1;
			}
			internal set {
				xButton1 = value;
			}
		}
		public ButtonState XButton2 {
			get {
				return xButton2;
			}
			internal set {
				xButton2 = value;
			}
		}
	}
}

