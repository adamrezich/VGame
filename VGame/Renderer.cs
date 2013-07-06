using System;
using System.Runtime.InteropServices;
using Tao.Sdl;
using Cairo;

namespace VGame {
	public class Renderer {
		Game game;
		IntPtr sdlBuffer;
		IntPtr surfacePtr;
		int bpp = 32;
		int width = 1280;
		int height = 720;
		int resultFlip;
		ImageSurface imgSurface;
		bool borderless = false;
		public int Width {
			get {
				return width;
			}
		}
		public int Height {
			get {
				return height;
			}
		}
		public bool Borderless {
			get {
				return borderless;
			}
		}

		public Renderer(Game game, int width, int height, bool borderless) {
			this.game = game;
			this.width = width;
			this.height = height;
			this.borderless = borderless;

			Initialize();
		}
		~Renderer() {
			Sdl.SDL_FreeSurface(surfacePtr);
		}
		public void Clear(Context g) {
			g.Color = new Cairo.Color(1, 0, 0);
			g.Paint();
		}
		public void Draw() {
			using (Context g = new Context(imgSurface)) {
				Clear(g);
				game.Draw(g);
			}
			resultFlip = Sdl.SDL_Flip(surfacePtr);
		}
		public void Close() {
			Sdl.SDL_Quit();
		}

		protected void Initialize() {
			int init, flags;

			Sdl.SDL_putenv("SDL_VIDEO_CENTERED=center");
			init = Sdl.SDL_Init(Sdl.SDL_INIT_VIDEO);
			flags = (Sdl.SDL_SWSURFACE | Sdl.SDL_DOUBLEBUF | Sdl.SDL_ANYFORMAT);
			if (borderless)
				flags = flags | Sdl.SDL_NOFRAME;
			surfacePtr = Sdl.SDL_SetVideoMode(width, height, bpp, flags);

			Sdl.SDL_Surface surface = (Sdl.SDL_Surface)Marshal.PtrToStructure(surfacePtr, typeof(Sdl.SDL_Surface));
			sdlBuffer = surface.pixels;

			imgSurface = new ImageSurface(sdlBuffer, Format.RGB24, width, height, width * 4);
		}
	}
}

