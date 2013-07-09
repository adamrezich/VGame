using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Tao.Sdl;
using Cairo;

namespace VGame {
	public class Renderer : IDisposable {
		public static Renderer Current = null;
		Game game;
		IntPtr sdlBuffer;
		IntPtr surfacePtr;
		int bpp = 32;
		int width = 1280;
		int height = 720;
		int resultFlip;
		int flags = 0;
		ImageSurface imgSurface;
		bool borderless = false;
		bool fullscreen = false;
		bool antialiasing = true;
		Context context;
		bool isDisposing = false;
		int fps = 0;
		int totalFrames = 0;
		double elapsedTime = 0;
		List<Rectangle> resolutions = new List<Rectangle>();
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
		public bool Fullscreen {
			get {
				return fullscreen;
			}
		}
		public bool Antialiasing {
			get {
				return antialiasing;
			}
			set {
				antialiasing = value;
			}
		}
		public Context Context {
			get {
				return context;
			}
		}
		public int FPS {
			get {
				return fps;
			}
		}
		public List<Rectangle> Resolutions {
			get {
				if (resolutions.Count == 0) {
					Sdl.SDL_Surface surface = (Sdl.SDL_Surface)Marshal.PtrToStructure(surfacePtr, typeof(Sdl.SDL_Surface));
					Sdl.SDL_Rect[] rects = Sdl.SDL_ListModes(surface.format, Sdl.SDL_HWSURFACE | Sdl.SDL_FULLSCREEN);
					foreach (Sdl.SDL_Rect r in rects) {
						resolutions.Add(new Rectangle(r.x, r.y, r.w, r.h));
					}
				}
				return resolutions;
			}
		}

		public Renderer(Game game, int width, int height, bool fullscreen, bool borderless) {
			this.game = game;
			this.width = width;
			this.height = height;
			this.fullscreen = fullscreen;
			this.borderless = borderless;

			Initialize();
			Current = this;
		}
		~Renderer() {
			Dispose(false);
		}
		
		public void Dispose() {
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}
		protected virtual void Dispose(bool disposing) {
			if (!this.isDisposing) {
				if (disposing) {
					if (surfacePtr != IntPtr.Zero && surfacePtr != null)
					    Sdl.SDL_FreeSurface(surfacePtr);
					surfacePtr = IntPtr.Zero;
					Sdl.SDL_Quit();
					if (context.Target != null)
						((IDisposable)context.Target).Dispose();
					if (context != null)
						((IDisposable)context).Dispose();
				}
				this.isDisposing = true;
			}
		}

		public void Antialias() {
			context.Antialias = antialiasing ? Cairo.Antialias.Subpixel : Cairo.Antialias.None;
		}
		public void Clear() {
			Clear(ColorPresets.Black);
		}
		public void Clear(Color color) {
			context.Color = color;
			context.Paint();
		}
		public void Draw(GameTime gameTime) {
			Clear();
			game.Draw(gameTime);
			resultFlip = Sdl.SDL_Flip(surfacePtr);
		}
		public void Close() {
			Dispose(true);
		}
		public void AddFrame(GameTime gameTime) {
			elapsedTime += gameTime.ElapsedGameTime.TotalMilliseconds;
			totalFrames++;
			if (elapsedTime >= 1000) {
				fps = totalFrames;
				totalFrames = 0;
				elapsedTime = 0;
			}
		}

		public void DrawText(Vector2 position, string text, double scale, TextAlign hAlign, TextAlign vAlign, Cairo.Color? fillColor, Cairo.Color? strokeColor, Cairo.Color? backgroundColor, double angle, string font) {
			Context g = context;
			int TextBoxPadding = 4;
			if (font == null)
				font = "04b_19";
			g.SelectFontFace(font, FontSlant.Normal, FontWeight.Normal);
			g.SetFontSize(scale);
			TextExtents ext = g.TextExtents(text);
			TextExtents ext2 = g.TextExtents("|");
			Vector2 offset = new Vector2(0, 0);
			switch (hAlign) {
				case TextAlign.Left:
					break;
					case TextAlign.Center:
					offset.X = -(float)(ext.Width / 2);
					break;
					case TextAlign.Right:
					offset.X = -(float)(ext.Width);
					break;
			}
			switch (vAlign) {
				case TextAlign.Top:
					break;
					case TextAlign.Middle:
					offset.Y = -(float)(ext2.Height / 2);
					break;
					case TextAlign.Bottom:
					offset.Y = -(float)(ext2.Height);
					break;
			}
			Vector2 textPos = position - new Vector2((float)(ext.XBearing), (float)(ext2.YBearing)) + offset;
			Vector2 boxOffset = new Vector2((float)(ext.XBearing), (float)(-ext2.Height));
			if (backgroundColor.HasValue) {
				g.MoveTo((textPos + boxOffset + new Vector2(-TextBoxPadding, -TextBoxPadding)).ToPointD());
				g.LineTo((textPos + boxOffset + new Vector2((float)ext.Width, 0) + new Vector2(TextBoxPadding, -TextBoxPadding)).ToPointD());
				g.LineTo((textPos + boxOffset + new Vector2((float)ext.Width, (float)ext.Height) + new Vector2(TextBoxPadding, TextBoxPadding)).ToPointD());
				g.LineTo((textPos + boxOffset + new Vector2(0, (float)ext.Height) + new Vector2(-TextBoxPadding, TextBoxPadding)).ToPointD());
				g.ClosePath();
				g.Color = (Cairo.Color)backgroundColor;
				g.Fill();
			}
			if (fillColor.HasValue) {
				g.MoveTo(textPos.ToPointD());
				g.Color = (Cairo.Color)fillColor;
				if (angle != 0) g.Rotate(angle);
				g.ShowText(text);
			}
			if (strokeColor.HasValue) {
				g.Antialias = Cairo.Antialias.None;
				g.MoveTo(textPos.ToPointD());
				g.Color = (Cairo.Color)strokeColor;
				g.LineWidth = 1;
				g.TextPath(text);
				if (angle != 0) g.Rotate(angle);
				g.Stroke();
				g.LineWidth = 2;
			}
			Antialias();
		}

		protected void Initialize() {
			int init;

			Sdl.SDL_putenv("SDL_VIDEO_CENTERED=center");
			init = Sdl.SDL_Init(Sdl.SDL_INIT_VIDEO);
			flags = Sdl.SDL_SWSURFACE | Sdl.SDL_DOUBLEBUF | Sdl.SDL_ANYFORMAT | (fullscreen ? Sdl.SDL_FULLSCREEN : 0) | (borderless ? Sdl.SDL_NOFRAME : 0);
			surfacePtr = Sdl.SDL_SetVideoMode(width, height, bpp, flags);

			Sdl.SDL_Surface surface = (Sdl.SDL_Surface)Marshal.PtrToStructure(surfacePtr, typeof(Sdl.SDL_Surface));
			sdlBuffer = surface.pixels;

			imgSurface = new ImageSurface(sdlBuffer, Format.RGB24, width, height, width * 4);
			context = new Context(imgSurface);
		}
	}
}

