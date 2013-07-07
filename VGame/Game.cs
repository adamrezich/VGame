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
			Renderer = new Renderer(this, 1280, 720, true);
			InputManager = new InputManager(this);
			StateManager = new StateManager(this);
			Initialize();
		}

		public virtual void Initialize() {
		}
		public virtual void HandleInput() {
		}
		public virtual void Update() {
			StateManager.Update();
		}
		public virtual void Draw(Context g) {
			StateManager.Draw(g);
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

		protected void Tick() {
			InputManager.Tick();
			Update();
			PollEvents();
			Renderer.Draw();
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

