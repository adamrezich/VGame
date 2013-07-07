using System;
using System.Collections.Generic;
using Tao.Sdl;

namespace VGame {
	public class InputManager {
		Game game;
		MouseState mouseState;
		MouseState lastMouseState;
		Dictionary<MouseButton, bool> downMouseButtons = new Dictionary<MouseButton, bool>();

		public InputManager(Game game) {
			this.game = game;
			downMouseButtons.Add(MouseButton.Left, false);
			downMouseButtons.Add(MouseButton.Middle, false);
			downMouseButtons.Add(MouseButton.Right, false);
			downMouseButtons.Add(MouseButton.ScrollUp, false);
			downMouseButtons.Add(MouseButton.ScrollDown, false);
			downMouseButtons.Add(MouseButton.XButton1, false);
			downMouseButtons.Add(MouseButton.XButton2, false);
			mouseState = new MouseState(0, 0, ButtonState.Up, ButtonState.Up, ButtonState.Up, ButtonState.Up, ButtonState.Up, ButtonState.Up, ButtonState.Up);
			lastMouseState = new MouseState(0, 0, ButtonState.Up, ButtonState.Up, ButtonState.Up, ButtonState.Up, ButtonState.Up, ButtonState.Up, ButtonState.Up);
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
		public void Tick() {
			lastMouseState = mouseState;
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
			}
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
		RightShift = Sdl.SDLK_RSHIFT
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

