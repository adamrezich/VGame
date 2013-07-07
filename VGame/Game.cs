using System;
using System.Collections.Generic;
using Tao.Sdl;
using Cairo;

namespace VGame {
	public class Game {
		public Renderer Renderer;
		public InputManager InputManager;
		public StateManager StateManager;
		public bool IsExiting {
			get {
				return exiting;
			}
		}
		protected bool exiting = false;
		public bool IsActive {
			get {
				// TODO: Implement
				return true;
			}
		}

		public Game() {
			Renderer = new Renderer(this, 1280, 720, false);
			InputManager = new InputManager(this);
			StateManager = new StateManager(this);
			Initialize();
		}

		protected virtual void Initialize() {
		}
		protected virtual void HandleInput() {
		}
		protected virtual void Update(GameTime gameTime) {
			StateManager.Update(gameTime);
		}
		public virtual void Draw(GameTime gameTime) {
			StateManager.Draw(gameTime);
		}
		public void Run() {
			while (!exiting) {
				Tick();
			}
			Renderer.Close();
		}
		public void Exit() {
			exiting = true;
		}

		public string WindowCaption {
			get {
				string title, icon;
				Sdl.SDL_WM_GetCaption(out title, out icon);
				return title;
			}
			set {
				Sdl.SDL_WM_SetCaption(value, null);
			}
		}
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

		protected void Tick() {
			// TODO: Make this actually do something!
			GameTime gt = new GameTime();
			InputManager.Tick();
			Update(gt);
			PollEvents();
			Renderer.Draw(gt);
		}
		protected void PollEvents() {
			Sdl.SDL_Event e;
			while (Sdl.SDL_PollEvent(out e) == 1) {
				InputManager.HandleEvent(e);
				switch (e.type) {
					case Sdl.SDL_QUIT:
						Exit();
						break;
					case Sdl.SDL_KEYDOWN:
						if (e.key.keysym.sym == Sdl.SDLK_ESCAPE) {
							Exit();
						}
						break;
				}
			}
		}
	}
}

