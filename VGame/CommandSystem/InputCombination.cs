using System;
using System.Collections.Generic;
using System.Linq;

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

		public static InputCombination Parse(string comboString) {
			if (comboString == "")
				throw new Exception("Malformed binding: empty binding.");
			List<string> split = new List<string>(comboString.Split(new char[] { '+' }, StringSplitOptions.RemoveEmptyEntries));
			bool control = false;
			bool shift = false;
			bool alt = false;
			if (split.Contains("Ctrl")) {
				control = true;
				split.Remove("Ctrl");
			}
			if (split.Contains("Shift")) {
				shift = true;
				split.Remove("Shift");
			}
			if (split.Contains("Alt")) {
				alt = true;
				split.Remove("Alt");
			}
			if (split.Count == 0) {
				// This shouldn't happen. If the user was trying to bind one of the modifier keys to a command (i.e.
				// Ctrl to crouch), they should've used the specific key name for it (i.e. LeftControl)
				throw new Exception(string.Format("Malformed binding: \"{0}\" is not valid because it contains modifiers, but no actual input."));
			}
			if (split.Count > 1) {
				// This shouldn't happen, either. After we've removed all the modifiers, if there's more than one thing
				// left, it's wrong (i.e. user tried to bind "Ctrl+Shift+RandomBullshit+F")
				throw new Exception(string.Format("Malformed binding: \"{0}\" is not valid because it contains illegal modifiers or something."));
			}

			Keys key;
			if (Enum.TryParse<Keys>(split[0], true, out key)) {
				return new KeyCombination(key, control, shift, alt);
			}
			MouseButton button;
			if (Enum.TryParse<MouseButton>(split[0], true, out button)) {
			}
			return null;
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
		public override string ToString() {
			return string.Format("{1}{2}{3}{0}", key, control ? "Ctrl+" : "", shift ? "Shift+" : "", alt ? "Alt+" : "");
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
		public override string ToString() {
			return string.Format("{1}{2}{3}{0}", button, control ? "Ctrl+" : "", shift ? "Shift+" : "", alt ? "Alt+" : "");
		}
	}
}

