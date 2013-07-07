using System;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;
using Tao.Sdl;
using Cairo;

namespace VGame {
	public class Game {
		public Renderer Renderer;
		public InputManager InputManager;
		public StateManager StateManager;
		private TimeSpan _targetElapsedTime = TimeSpan.FromTicks((long)10000000 / (long)60f);
		private TimeSpan _inactiveSleepTime = TimeSpan.FromSeconds(1);
		private readonly TimeSpan _maxElapsedTime = TimeSpan.FromMilliseconds(500);
		private TimeSpan _accumulatedElapsedTime;
		private readonly GameTime _gameTime = new GameTime();
		private Stopwatch _gameTimer = Stopwatch.StartNew();

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
			while (!IsExiting)
				Tick();
			Renderer.Close();
		}

		public void Exit() {
			exiting = true;
		}

		public void Tick() {
		RetryTick:
			_accumulatedElapsedTime += _gameTimer.Elapsed;
			_gameTimer.Reset();
			_gameTimer.Start();
			if (_accumulatedElapsedTime < _targetElapsedTime) {
				var sleepTime = (int)(_targetElapsedTime - _accumulatedElapsedTime).TotalMilliseconds;
				System.Threading.Thread.Sleep(sleepTime);
				goto RetryTick;
			}
			if (_accumulatedElapsedTime > _maxElapsedTime)
				_accumulatedElapsedTime = _maxElapsedTime;
			_gameTime.ElapsedGameTime = _targetElapsedTime;
			var stepCount = 0;
			_gameTime.IsRunningSlowly = (_accumulatedElapsedTime > _targetElapsedTime);
			while (_accumulatedElapsedTime >= _targetElapsedTime) {
				_gameTime.TotalGameTime += _targetElapsedTime;
				_accumulatedElapsedTime -= _targetElapsedTime;
				++stepCount;
				PollEvents();
				InputManager.Tick();
				Update(_gameTime);
			}
			Renderer.Draw(_gameTime);
		}

		public void ResetElapsedTime() {
			_gameTimer.Reset();
			_gameTimer.Start();
			_accumulatedElapsedTime = TimeSpan.Zero;
			_gameTime.ElapsedGameTime = TimeSpan.Zero;
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

