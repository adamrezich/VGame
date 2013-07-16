using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tao.Sdl;

namespace VGame {
	public class InputManager {
		MouseState mouseState;
		MouseState lastMouseState;
		KeyboardState keyboardState;
		KeyboardState lastKeyboardState;
		Dictionary<MouseButton, bool> downMouseButtons = new Dictionary<MouseButton, bool>();
		Dictionary<Keys, bool> downKeys = new Dictionary<Keys, bool>();
		List<char> newUnicode = new List<char>();

		public InputManager() {
			downMouseButtons.Add(MouseButton.Left, false);
			downMouseButtons.Add(MouseButton.Middle, false);
			downMouseButtons.Add(MouseButton.Right, false);
			downMouseButtons.Add(MouseButton.ScrollUp, false);
			downMouseButtons.Add(MouseButton.ScrollDown, false);
			downMouseButtons.Add(MouseButton.XButton1, false);
			downMouseButtons.Add(MouseButton.XButton2, false);
			mouseState = new MouseState(0, 0, ButtonState.Up, ButtonState.Up, ButtonState.Up, ButtonState.Up, ButtonState.Up, ButtonState.Up, ButtonState.Up);
			lastMouseState = new MouseState(0, 0, ButtonState.Up, ButtonState.Up, ButtonState.Up, ButtonState.Up, ButtonState.Up, ButtonState.Up, ButtonState.Up);
			keyboardState = new KeyboardState();
			lastKeyboardState = new KeyboardState();
		}
		public bool IsShiftKeyDown {
			get {
				return KeyDown(Keys.LeftShift) || KeyDown(Keys.RightShift);
			}
		}
		public bool IsControlKeyDown {
			get {
				return KeyDown(Keys.LeftControl) || KeyDown(Keys.RightControl);
			}
		}
		public bool IsAltKeyDown {
			get {
				return KeyDown(Keys.LeftAlt) || KeyDown(Keys.RightAlt);
			}
		}
		public ButtonState KeyState(Keys key) {
			if (keyboardState.Keys.ContainsKey(key)) {
				return keyboardState.Keys[key];
			}
			else
				return ButtonState.Up;
		}
		public bool KeyDown(Keys key) {
			ButtonState state = KeyState(key);
			return state == ButtonState.Down || state == ButtonState.Pressed;
		}
		public List<char> GetTextInput() {
			return keyboardState.Unicode;
		}
		public ButtonState MouseButtonState(MouseButton button) {
			switch (button) {
				case MouseButton.Left:
					return mouseState.LeftButton;
				case MouseButton.Middle:
					return mouseState.MiddleButton;
				case MouseButton.Right:
					return mouseState.RightButton;
				case MouseButton.ScrollDown:
					return mouseState.ScrollWheelDown;
				case MouseButton.ScrollUp:
					return mouseState.ScrollWheelUp;
				case MouseButton.XButton1:
					return mouseState.XButton1;
				case MouseButton.XButton2:
					return mouseState.XButton2;
			}
			return ButtonState.Up;
		}
		public Point MousePosition {
			get {
				return mouseState.Position;
			}
		}
		public Point MousePositionLast {
			get {
				return lastMouseState.Position;
			}
		}
		public bool MouseMoved {
			get {
				return mouseState.Position != lastMouseState.Position;
			}
		}
		public void Tick() {
			lastMouseState = mouseState;
			lastKeyboardState.Keys = new Dictionary<Keys, ButtonState>(keyboardState.Keys);
			int x, y;
			Sdl.SDL_GetMouseState(out x, out y);
			mouseState = new MouseState(x, y,
			                            NewButtonState(lastMouseState.ScrollWheelUp, downMouseButtons[MouseButton.ScrollUp]),
			                            NewButtonState(lastMouseState.ScrollWheelDown, downMouseButtons[MouseButton.ScrollDown]),
			                            NewButtonState(lastMouseState.LeftButton, downMouseButtons[MouseButton.Left]),
			                            NewButtonState(lastMouseState.MiddleButton, downMouseButtons[MouseButton.Middle]),
			                            NewButtonState(lastMouseState.RightButton, downMouseButtons[MouseButton.Right]),
			                            NewButtonState(lastMouseState.XButton1, downMouseButtons[MouseButton.XButton1]),
			                            NewButtonState(lastMouseState.XButton2, downMouseButtons[MouseButton.XButton2]));

			keyboardState.Keys.Clear();
			keyboardState.Unicode = new List<char>(newUnicode);
			foreach (KeyValuePair<Keys, bool> kvp in downKeys) {
				if (lastKeyboardState.Keys.ContainsKey(kvp.Key))
					keyboardState.Keys.Add(kvp.Key, NewButtonState(lastKeyboardState.Keys[kvp.Key], kvp.Value));
				else
					keyboardState.Keys.Add(kvp.Key, ButtonState.Pressed);
			}
			foreach (KeyValuePair<Keys, ButtonState> kvp in lastKeyboardState.Keys)
				if (!keyboardState.Keys.ContainsKey(kvp.Key) && kvp.Value != ButtonState.Up)
					keyboardState.Keys.Add(kvp.Key, kvp.Value == ButtonState.Pressed ? ButtonState.Down : kvp.Value == ButtonState.Released ? ButtonState.Up : kvp.Value);

			downKeys.Clear();
			newUnicode.Clear();
		}
		public void HandleEvent(Sdl.SDL_Event e) {
			switch (e.type) {
				case Sdl.SDL_MOUSEBUTTONDOWN:
					switch (e.button.button) {
						case Sdl.SDL_BUTTON_LEFT:
							downMouseButtons[MouseButton.Left] = true;
							break;
						case Sdl.SDL_BUTTON_MIDDLE:
							downMouseButtons[MouseButton.Middle] = true;
							break;
						case Sdl.SDL_BUTTON_RIGHT:
							downMouseButtons[MouseButton.Right] = true;
							break;
						case Sdl.SDL_BUTTON_WHEELUP:
							downMouseButtons[MouseButton.ScrollUp] = true;
							break;
						case Sdl.SDL_BUTTON_WHEELDOWN:
							downMouseButtons[MouseButton.ScrollDown] = true;
							break;
						case Sdl.SDL_BUTTON_X1:
							downMouseButtons[MouseButton.XButton1] = true;
							break;
						case Sdl.SDL_BUTTON_X2:
							downMouseButtons[MouseButton.XButton2] = true;
							break;
					}
					break;
				case Sdl.SDL_MOUSEBUTTONUP:
					switch (e.button.button) {
						case Sdl.SDL_BUTTON_LEFT:
							downMouseButtons[MouseButton.Left] = false;
							break;
						case Sdl.SDL_BUTTON_MIDDLE:
							downMouseButtons[MouseButton.Middle] = false;
							break;
						case Sdl.SDL_BUTTON_RIGHT:
							downMouseButtons[MouseButton.Right] = false;
							break;
						case Sdl.SDL_BUTTON_WHEELUP:
							downMouseButtons[MouseButton.ScrollUp] = false;
							break;
						case Sdl.SDL_BUTTON_WHEELDOWN:
							downMouseButtons[MouseButton.ScrollDown] = false;
							break;
						case Sdl.SDL_BUTTON_X1:
							downMouseButtons[MouseButton.XButton1] = false;
							break;
						case Sdl.SDL_BUTTON_X2:
							downMouseButtons[MouseButton.XButton2] = false;
							break;
					}
					break;
				case Sdl.SDL_KEYDOWN:
					if (((IList)Enum.GetValues(typeof(Keys))).Contains((Keys)e.key.keysym.sym)) {
						if (downKeys.ContainsKey((Keys)e.key.keysym.sym))
							downKeys[(Keys)e.key.keysym.sym] = true;
						else
							downKeys.Add((Keys)e.key.keysym.sym, true);
					}
					if (IsValidUnicode((char)e.key.keysym.unicode))
						newUnicode.Add((char)e.key.keysym.unicode);
					break;
				case Sdl.SDL_KEYUP:
					if (((IList)Enum.GetValues(typeof(Keys))).Contains((Keys)e.key.keysym.sym)) {
						if (downKeys.ContainsKey((Keys)e.key.keysym.sym))
							downKeys[(Keys)e.key.keysym.sym] = false;
						else
							downKeys.Add((Keys)e.key.keysym.sym, false); // how did this happen?
					}
					break;
			}
		}
		protected bool IsValidUnicode(char c) {
			// hopefully this isn't bad
			return (char.IsLetterOrDigit(c) || char.IsPunctuation(c) || char.IsWhiteSpace(c) || Char.IsSymbol(c)) && !char.IsControl(c);
		}
		protected ButtonState NewButtonState(ButtonState state, bool down) {
			switch (state) {
				case ButtonState.Up:
					return down ? ButtonState.Pressed : ButtonState.Up;
				case ButtonState.Pressed:
					return down ? ButtonState.Down : ButtonState.Released;
				case ButtonState.Down:
					return down ? ButtonState.Down : ButtonState.Released;
				case ButtonState.Released:
					return down ? ButtonState.Pressed : ButtonState.Up;
			}
			return ButtonState.Up;
		}
	}

	public enum ButtonState {
		Up,
		Released,
		Pressed,
		Down
	}
	public enum MouseButton {
		Left,
		Middle,
		Right,
		ScrollUp,
		ScrollDown,
		XButton1,
		XButton2
	}
	public enum Keys {
		Up = Sdl.SDLK_UP,
		Down = Sdl.SDLK_DOWN,
		Left = Sdl.SDLK_LEFT,
		Right = Sdl.SDLK_RIGHT,
		Enter = Sdl.SDLK_RETURN,
		Space = Sdl.SDLK_SPACE,
		Escape = Sdl.SDLK_ESCAPE,
		Tab = Sdl.SDLK_TAB,
		LeftControl = Sdl.SDLK_LCTRL,
		RightControl = Sdl.SDLK_RCTRL,
		LeftAlt = Sdl.SDLK_LALT,
		RightAlt = Sdl.SDLK_RALT,
		LeftShift = Sdl.SDLK_LSHIFT,
		RightShift = Sdl.SDLK_RSHIFT,
		Backspace = Sdl.SDLK_BACKSPACE,
		A = Sdl.SDLK_a,
		B = Sdl.SDLK_b,
		C = Sdl.SDLK_c,
		D = Sdl.SDLK_d,
		E = Sdl.SDLK_e,
		F = Sdl.SDLK_f,
		G = Sdl.SDLK_g,
		H = Sdl.SDLK_h,
		I = Sdl.SDLK_i,
		J = Sdl.SDLK_j,
		K = Sdl.SDLK_k,
		L = Sdl.SDLK_l,
		M = Sdl.SDLK_m,
		N = Sdl.SDLK_n,
		O = Sdl.SDLK_o,
		P = Sdl.SDLK_p,
		Q = Sdl.SDLK_q,
		R = Sdl.SDLK_r,
		S = Sdl.SDLK_s,
		T = Sdl.SDLK_t,
		U = Sdl.SDLK_u,
		V = Sdl.SDLK_v,
		W = Sdl.SDLK_w,
		X = Sdl.SDLK_x,
		Y = Sdl.SDLK_y,
		Z = Sdl.SDLK_z,
		D1 = Sdl.SDLK_1,
		D2 = Sdl.SDLK_2,
		D3 = Sdl.SDLK_3,
		D4 = Sdl.SDLK_4,
		D5 = Sdl.SDLK_5,
		D6 = Sdl.SDLK_6,
		D7 = Sdl.SDLK_7,
		D8 = Sdl.SDLK_8,
		D9 = Sdl.SDLK_9,
		D0 = Sdl.SDLK_0,
		F1 = Sdl.SDLK_F1,
		F2 = Sdl.SDLK_F2,
		F3 = Sdl.SDLK_F3,
		F4 = Sdl.SDLK_F4,
		F5 = Sdl.SDLK_F5,
		F6 = Sdl.SDLK_F6,
		F7 = Sdl.SDLK_F7,
		F8 = Sdl.SDLK_F8,
		F9 = Sdl.SDLK_F9,
		F10 = Sdl.SDLK_F10,
		F11 = Sdl.SDLK_F11,
		F12 = Sdl.SDLK_F12,
		Insert = Sdl.SDLK_INSERT,
		Delete = Sdl.SDLK_DELETE,
		Home = Sdl.SDLK_HOME,
		End = Sdl.SDLK_END,
		PageUp = Sdl.SDLK_PAGEUP,
		PageDown = Sdl.SDLK_PAGEDOWN,
		Comma = Sdl.SDLK_COMMA,
		Period = Sdl.SDLK_PERIOD,
		ForwardSlash = Sdl.SDLK_SLASH,
		Backslash = Sdl.SDLK_BACKSLASH,
		Backquote = Sdl.SDLK_BACKQUOTE,
		Semicolon = Sdl.SDLK_SEMICOLON
	}
	public enum KeyModifiers {
		None = Sdl.KMOD_NONE,
		Numlock = Sdl.KMOD_NUM,
		CapsLock = Sdl.KMOD_CAPS,
		LeftControl = Sdl.KMOD_LCTRL,
		RightControl = Sdl.KMOD_RCTRL,
		RightShift = Sdl.KMOD_RSHIFT,
		LeftShift = Sdl.KMOD_LSHIFT,
		RightAlt = Sdl.KMOD_RALT,
		LeftAlt = Sdl.KMOD_LALT,
		Control = Sdl.KMOD_CTRL,
		Shift = Sdl.KMOD_SHIFT,
		Alt = Sdl.KMOD_ALT
	}
}

