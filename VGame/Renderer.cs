using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Tao.Sdl;
using Cairo;

namespace VGame {
	public class Renderer : IDisposable {
		public static Renderer Current = null;
		Game game;
		IntPtr surfacePtr;
		int bpp = 32;
		int width = 1280;
		int height = 720;
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
		public bool IsReady {
			get {
				return ready;
			}
		}
		private bool ready = false;
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
		private Dictionary<string, Font> fonts = new Dictionary<string, Font>();

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
					if (surfacePtr != IntPtr.Zero)
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
			if (Sdl.SDL_Flip(surfacePtr) != 0)
				throw new Exception("Failed to swap buffers!");
		}
		public void AddFrame(GameTime gameTime) {
			elapsedTime += gameTime.ElapsedGameTime.TotalMilliseconds;
			if (elapsedTime >= 1000) {
				fps = totalFrames;
				totalFrames = 0;
				elapsedTime = 0;
			}
			else
				totalFrames++;
		}
		public Rectangle DrawText(Vector2 position, string text, double scale, TextAlign hAlign, TextAlign vAlign, Cairo.Color? fillColor, Cairo.Color? strokeColor, Cairo.Color? backgroundColor, double angle, string font) {
			return DrawText(position, text, scale, hAlign, vAlign, fillColor, strokeColor, backgroundColor, angle, font, 2);
		}
		public Rectangle DrawText(Vector2 position, string text, double scale, TextAlign hAlign, TextAlign vAlign, Cairo.Color? fillColor, Cairo.Color? strokeColor, Cairo.Color? backgroundColor, double angle, string font, int padding) {
			Context g = context;
			if (font == null)
				font = "console";
			//g.SelectFontFace(font, FontSlant.Normal, FontWeight.Normal);
			SetFont(font);
			g.SetFontSize(scale);
			TextExtents ext = g.TextExtents(text);
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
					offset.Y = -(float)(g.FontExtents.Height / 2);
					break;
					case TextAlign.Bottom:
					offset.Y = -(float)(g.FontExtents.Height);
					break;
			}
			Vector2 baseline = position + new Vector2(0, (float)g.FontExtents.Ascent);
			Vector2 textPos = baseline + offset;
			Rectangle r = new Rectangle((int)(position.X - padding + offset.X), (int)(position.Y - padding + offset.Y), (int)(ext.Width + padding * 2), (int)(g.FontExtents.Height + padding * 2));
			if (backgroundColor.HasValue) {
				g.MoveTo(r.Left, r.Top);
				g.LineTo(r.Right, r.Top);
				g.LineTo(r.Right, r.Bottom);
				g.LineTo(r.Left, r.Bottom);
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
				if (fonts[font].Antialiased)
					Antialias();
				else
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
			return r;
		}
		public void LoadFont(string name, string filename) {
			LoadFont(name, filename, false);
		}
		public void LoadFont(string name, string filename, bool antialiased) {
			if (!System.IO.File.Exists(filename))
				throw new Exception(string.Format("Font file \"{0}\" is missing!", filename));
			FontOptions opt = new FontOptions();
			if (antialiased)
				opt.Antialias = Cairo.Antialias.Subpixel;
			else
				opt.Antialias = Cairo.Antialias.None;
			opt.HintStyle = HintStyle.Slight;
			fonts.Add(name, new Font(new ScaledFont(FreeTypeFontFace.Create(filename, 0, 2), new Matrix(), Context.Matrix, opt), antialiased));
		}
		public void SetFont(string font) {
			cairo_set_scaled_font(Context.Handle, fonts[font].ScaledFont.Handle);
		}

		protected void Initialize() {
			Sdl.SDL_putenv("SDL_VIDEO_CENTERED=center");
			if (Sdl.SDL_Init(Sdl.SDL_INIT_VIDEO) != 0) {
				throw new Exception("Video failed to initialize!");
			}
			flags = Sdl.SDL_SWSURFACE | Sdl.SDL_DOUBLEBUF | Sdl.SDL_ANYFORMAT | (fullscreen ? Sdl.SDL_FULLSCREEN : 0) | (borderless ? Sdl.SDL_NOFRAME : 0);
			surfacePtr = IntPtr.Zero;
			surfacePtr = Sdl.SDL_SetVideoMode(width, height, bpp, flags);
			if (surfacePtr == IntPtr.Zero)
				throw new Exception("Failed to set the surface pointer to point to the surface video mode thing!");

			Sdl.SDL_Surface surface = (Sdl.SDL_Surface)Marshal.PtrToStructure(surfacePtr, typeof(Sdl.SDL_Surface));
			IntPtr sdlBuffer = IntPtr.Zero;
			sdlBuffer = surface.pixels;
			if (sdlBuffer == IntPtr.Zero)
				throw new Exception("Failed to set the surface buffer pointer to point to the surface!");

			imgSurface = new ImageSurface(sdlBuffer, Format.Argb32, width, height, width * 4);
			context = new Context(imgSurface);
			ready = true;
		}

		[DllImport ("libcairo-2.dll")]
		private static extern int cairo_set_scaled_font(IntPtr cr, IntPtr cr_scaled_font);
	}
}