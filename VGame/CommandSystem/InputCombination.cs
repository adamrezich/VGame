using System;

namespace VGame.CommandSystem {
	public abstract class InputCombination {
		public bool Control {
			get {
				return control;
			}
		}
		protected bool control;
		public bool Shift {
			get {
				return shift;
			}
		}
		protected bool shift;
		public bool Alt {
			get {
				return alt;
			}
		}
		protected bool alt;

		public abstract bool IsDown(InputManager inputManager);
		public abstract bool IsPressed(InputManager inputManager);
		//public abstract bool IsReleased { get; }

		protected bool IsModifierDown(InputManager inputManager) {
			return (control == inputManager.IsControlKeyDown) && (alt == inputManager.IsAltKeyDown) && (shift == inputManager.IsShiftKeyDown);
		}

		public static InputCombination Create(object o, bool control, bool shift, bool alt) {
			if (o is Keys)
				return new KeyCombination((Keys)o, control, shift, alt);
			if (o is MouseButton)
				return new MouseCombination((MouseButton)o, control, shift, alt);
			throw new Exception("Input object was neither a key press nor mouse button.");
		}
	}
	public class KeyCombination : InputCombination {
		public Keys Key {
			get {
				return key;
			}
		}
		protected Keys key;
		public override bool IsDown(InputManager inputManager) {
			return (inputManager.KeyState(key) == ButtonState.Down || inputManager.KeyState(key) == ButtonState.Pressed) && IsModifierDown(inputManager);
		}
		public override bool IsPressed(InputManager inputManager) {
			return inputManager.KeyState(key) == ButtonState.Pressed && IsModifierDown(inputManager);
		}
		/*public override bool IsReleased {
			get {
				throw new NotImplementedException();
			}
		}*/

		public KeyCombination(Keys key, bool control, bool shift, bool alt) {
			this.key = key;
			this.control = control;
			this.shift = shift;
			this.alt = alt;
		}
	}
	public class MouseCombination : InputCombination {
		public MouseButton Button {
			get {
				return button;
			}
		}
		protected MouseButton button;
		public override bool IsDown(InputManager inputManager) {
			return (inputManager.MouseButtonState(button) == ButtonState.Down || inputManager.MouseButtonState(button) == ButtonState.Pressed) && IsModifierDown(inputManager);
		}
		public override bool IsPressed(InputManager inputManager) {
			return inputManager.MouseButtonState(button) == ButtonState.Pressed && IsModifierDown(inputManager);
		}
		/*public override bool IsReleased {
			get {
				return inputManager.MouseButtonState(button) == ButtonState.Released && IsModifierDown;
			}
		}*/

		public MouseCombination(MouseButton button, bool control, bool shift, bool alt) {
			this.button = button;
			this.control = control;
			this.shift = shift;
			this.alt = alt;
		}
	}
}

