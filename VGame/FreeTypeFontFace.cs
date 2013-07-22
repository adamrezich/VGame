using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Cairo;

namespace VGame {

	public class FreeTypeInitException : Exception {
		public FreeTypeInitException() : base("Can't initialize freetype environment.") {
		}
	}

	public class CreateFaceException : Exception {
		public CreateFaceException(string filename) : base("Can't create the face for file: " + filename + ".") {
		}
	}

	public class LoadFaceException : Exception {
		public LoadFaceException(string filename) : base("Can't load the face for file: " + filename + ".") {
		}
	}

	public class FreeTypeFontFace : FontFace {
		private static Stack<IntPtr> fonts = new Stack<IntPtr>();
		private static bool initialized = false;
		private static IntPtr ft_lib;
		private IntPtr ft_face;

		private FreeTypeFontFace(IntPtr handler, IntPtr ft_face) : base(handler, true) {
			this.ft_face = ft_face;
		}

		protected override void Dispose(bool disposing) {
			fonts.Push(ft_face);
			base.Dispose(disposing);
		}

		public static void Cleanup() {
			while (fonts.Count > 0)
				FT_Done_Face(fonts.Pop());
		}

		public static FreeTypeFontFace Create(string filename, int faceindex, int loadoptions) {
			if (!initialized)
				initialize();
	 
			IntPtr ft_face;
			if (FT_New_Face(ft_lib, filename, faceindex, out ft_face) != 0)
				throw new LoadFaceException(filename);
	 
			IntPtr handler = cairo_ft_font_face_create_for_ft_face(ft_face, loadoptions);
			if (cairo_font_face_status(handler) != 0)
				throw new CreateFaceException(filename);
	 
			return new FreeTypeFontFace(handler, ft_face);
		}

		private static void initialize() {
			if (FT_Init_FreeType(out ft_lib) != 0)
				throw new FreeTypeInitException();
			initialized = true;
		}

		[DllImport ("freetype6.dll")]
		private static extern int FT_Init_FreeType(out IntPtr ft_lib);

		[DllImport ("freetype6.dll")]
		private static extern int FT_New_Face(IntPtr ft_lib, string filename, int faceindex, out IntPtr ft_face);

		[DllImport ("freetype6.dll")]
		private static extern int FT_Done_Face(IntPtr ft_face);

		[DllImport ("libcairo-2.dll")]
		private static extern IntPtr cairo_ft_font_face_create_for_ft_face(IntPtr ft_face, int loadoptions);

		[DllImport ("libcairo-2.dll")]
		private static extern int cairo_font_face_status(IntPtr cr_face);
	}
}