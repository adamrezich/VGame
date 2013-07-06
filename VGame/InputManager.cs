using System;
using System.Collections.Generic;
using Tao.Sdl;

namespace VGame {
	public class InputManager {
		Game game;
		Point mousePosition = new Point();
		bool mousePositionUpdated = false;

		public bool CursorVisible {
			get {
				return Sdl.SDL_ShowCursor(Sdl.SDL_QUERY) == Sdl.SDL_ENABLE;
			}
			set {
				Sdl.SDL_ShowCursor(value ? Sdl.SDL_ENABLE : Sdl.SDL_DISABLE);
			}
		}
		public bool ConstrainMouse {
			get {
				return Sdl.SDL_WM_GrabInput(Sdl.SDL_QUERY) == Sdl.SDL_GRAB_ON;
			}
			set {
				Sdl.SDL_WM_GrabInput(value ? Sdl.SDL_GRAB_ON : Sdl.SDL_GRAB_OFF);
			}
		}

		public Point MousePosition {
			get {
				if (!mousePositionUpdated) {
					int x, y;
					Sdl.SDL_GetMouseState(out x, out y);
					mousePosition.X = x;
					mousePosition.Y = y;
					mousePositionUpdated = true;
				}
				return mousePosition;
			}
		}

		public InputManager(Game game) {
			this.game = game;
		}
		public void Tick() {
			mousePositionUpdated = false;
		}
	}
}

