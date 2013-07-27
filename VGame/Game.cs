using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Reflection;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Tao.Sdl;
using Cairo;

using VGame.CommandSystem;
using VGame.StateSystem;

namespace VGame {
	public class Game : IDisposable {
		public Renderer Renderer = null;
		public InputManager InputManager;
		public StateManager StateManager;
		public CommandManager Cmd;
		private TimeSpan _targetElapsedTime = TimeSpan.FromTicks((long)10000000 / (long)60f);
		//private TimeSpan _inactiveSleepTime = TimeSpan.FromSeconds(1);
		private readonly TimeSpan _maxElapsedTime = TimeSpan.FromMilliseconds(500);
		private TimeSpan _accumulatedElapsedTime;
		private readonly GameTime _gameTime = new GameTime();
		private Stopwatch _gameTimer = Stopwatch.StartNew();
		public bool SuppressInput = false;
		private bool readyToUpdate = true;
		private bool drawingComplete = false;
		private int drawnFrames = 0;
		public bool ReadyToUpdate {
			get {
				return readyToUpdate && drawnFrames >= 1;
			}
		}
		private Thread drawThread = null;
		private bool useSeparateDrawThread = false;

		public double Zoom {
			get {
				return zoom;
			}
			set {
				zoom = value;
				if (Renderer != null)
					Renderer.Zoom = value;
			}
		}
		public double BaseSize {
			get {
				return baseSize;
			}
			set {
				baseSize = value;
				if (Renderer != null)
					Renderer.BaseSize = value;
			}
		}

		public bool IsExiting {
			get {
				return exiting;
			}
		}

		protected bool exiting = false;
		protected bool rendering = true;
		private double zoom;
		private double baseSize;

		public bool IsActive {
			get {
				// TODO: Implement
				return true;
			}
		}

		public Game() : this(true) {
		}
		public Game(bool initializeRenderer) {
			Zoom = 1;
			BaseSize = 1;
			if (initializeRenderer)
				ChangeResolution(new Rectangle(0, 0, 1024, 768), false, false, false);
			InputManager = new InputManager();
			StateManager = new StateManager(this);
			Cmd = new CommandManager(this);
			Initialize();
			Binding.Bind(InputCombination.Create(Keys.Escape, false, false, false), "escape");
			Binding.Bind(InputCombination.Create(Keys.Escape, true, false, true), "quit");
			Binding.Bind(InputCombination.Create(Keys.Up, false, false, false), "menu_up");
			Binding.Bind(InputCombination.Create(Keys.Down, false, false, false), "menu_down");
			Binding.Bind(InputCombination.Create(Keys.Tab, false, false, false), "menu_down");
			Binding.Bind(InputCombination.Create(Keys.Tab, false, true, false), "menu_up");
			Binding.Bind(InputCombination.Create(Keys.Space, false, false, false), "menu_select");
			Binding.Bind(InputCombination.Create(Keys.Enter, false, false, false), "menu_select");
			Binding.Bind(InputCombination.Create(Keys.Backquote, false, false, false), "console_toggle");
			Cmd.Console.WriteLine("Command console test");
			Cmd.Console.WriteLine("--------------------");
		}

		public void Dispose() {
			if (drawThread != null)
				drawThread.Join();
			if (Renderer != null)
				Renderer.Dispose();
		}

		protected virtual void Initialize() {
		}

		protected virtual void HandleInput() {
			bool wasActive = Cmd.Console.IsActive;
			Cmd.Console.HandleInput();
			SuppressInput = wasActive;
			foreach (Binding b in Binding.List)
				if (b.Combination.IsPressed(InputManager) && (!SuppressInput || b.Command == "console_toggle"))
					Cmd.Run(b.Command);
		}

		protected virtual void Update(GameTime gameTime) {
			StateManager.Update(gameTime);
		}

		public virtual void Draw(GameTime gameTime) {
			StateManager.Draw(gameTime);
			Cmd.Console.Draw(gameTime);
			if (Cmd.Variables["cl_showfps"].Value.BoolData)
				Renderer.DrawText(new Vector2(Renderer.Width - 8, 8), Renderer.FPS.ToString(), 28, TextAlign.Right, TextAlign.Top, Renderer.FPS >= 60 ? ColorPresets.White : Renderer.FPS > 45 ? ColorPresets.Yellow : ColorPresets.Red, ColorPresets.Black, null, 0, null);
		}

		public void Run() {
			Run(true);
		}
		public void Run(bool drawing) {
			while (!IsExiting) {
				Tick();
			}
			Dispose();
		}

		public void Exit() {
			StateManager.ClearStates();
			exiting = true;
			rendering = false;
		}

		public virtual bool IsClient() {
			return true;
		}

		public virtual bool IsServer() {
			return false;
		}

		protected virtual void LoadFonts() {
			Renderer.LoadFont("console", "ProFontWindows.ttf", true);
			Renderer.LoadFont("chunky", "ProFontWindows.ttf", true);
			Renderer.LoadFont("pixel", "ProFontWindows.ttf");
			Renderer.LoadFont("wide", "ProFontWindows.ttf", true);
		}

		private void BeginDrawLoop() {
			rendering = true;
			drawThread = new Thread(DrawLoop);
			drawThread.IsBackground = true;
			drawThread.Start();
		}
		public void DrawLoop() {
			while (rendering) {
				if (!readyToUpdate) {
					DrawTick();
				}
				else {
					drawingComplete = true;
				}
			}
		}

		public void DrawTick() {
			if (Renderer != null && ((Renderer.IsReady && drawnFrames < 1) || !useSeparateDrawThread)) {
				Draw(_gameTime);
				Renderer.Draw(_gameTime);
				Renderer.AddFrame(_gameTime);
				drawnFrames++;
			}
		}

		public void Tick() {
		RetryTick:
			_accumulatedElapsedTime += _gameTimer.Elapsed;
			_gameTimer.Reset();
			_gameTimer.Start();
			if (_accumulatedElapsedTime < _targetElapsedTime) {
				if (_accumulatedElapsedTime > _maxElapsedTime)
					_accumulatedElapsedTime = _maxElapsedTime;
				_gameTime.ElapsedGameTime = _targetElapsedTime;
				var sleepTime = (int)(_targetElapsedTime - _accumulatedElapsedTime).TotalMilliseconds;
				Thread.Sleep(sleepTime);
				goto RetryTick;
			}
			readyToUpdate = true;

			if (!drawingComplete && useSeparateDrawThread) {

				Thread.Sleep(1);
				goto RetryTick;
			}
			var stepCount = 0;
			_gameTime.IsRunningSlowly = (_accumulatedElapsedTime > _targetElapsedTime);
			while (_accumulatedElapsedTime >= _targetElapsedTime) {
				_gameTime.TotalGameTime += _targetElapsedTime;
				_accumulatedElapsedTime -= _targetElapsedTime;
				++stepCount;
				PollEvents();
				InputManager.Tick();
				HandleInput();
				Update(_gameTime);
			}
			if (!useSeparateDrawThread) {
				DrawTick();
			}
			drawingComplete = false;
			readyToUpdate = false;
			drawnFrames = 0;
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
				}
			}
		}

		public bool ChangeResolution(Rectangle resolution, bool fullscreen, bool borderless, bool doubleBuffered) {
			if (drawThread != null && useSeparateDrawThread) {
				rendering = false;
				drawThread.Join();
			}

			bool cursorVisible = CursorVisible;
			bool constrainMouse = ConstrainMouse;
			string windowCaption = WindowCaption;
			if (Renderer != null) {
				Renderer.Dispose();
			}
			Renderer = new Renderer(this, resolution.Width, resolution.Height, fullscreen, borderless, doubleBuffered);
			Renderer.Zoom = Zoom;
			Renderer.BaseSize = BaseSize;
			LoadFonts();
			Sdl.SDL_EnableUNICODE(Sdl.SDL_ENABLE);
			CursorVisible = cursorVisible;
			ConstrainMouse = constrainMouse;
			WindowCaption = windowCaption;
			if (useSeparateDrawThread)
				BeginDrawLoop();
			return true;
		}

		public void ErrorMessage(string message) {
			if (Cmd != null)
				Cmd.Console.WriteLine(message, ConsoleMessageType.Error);
		}
		public void WarningMessage(string message) {
			if (Cmd != null)
				Cmd.Console.WriteLine(message, ConsoleMessageType.Error);
		}

		public GameTime GetGameTime() {
			return _gameTime;
		}
	}
}

