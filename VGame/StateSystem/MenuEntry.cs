using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Cairo;

namespace VGame {
	public class MenuEntry {
		public Menu Menu;
		public string Text;
		public string Text2;
		public string Content;
		public Color DisabledFillColor = ColorPresets.Gray25;
		public Color NormalFillColor = ColorPresets.White;
		public Color SelectedFillColor = ColorPresets.Yellow;
		public Color LabelFillColor = ColorPresets.Gray75;
		public virtual Color FillColor {
			get {
				return Enabled ? IsSelected ? SelectedFillColor : NormalFillColor : DisabledFillColor;
			}
		}
		public virtual Color FillColor2 {
			get {
				return FillColor;
			}
		}
		public bool IsSelected = false;
		public bool Enabled {
			get {
				return selectable && Visible;
			}
			set {
				selectable = value;
			}
		}
		public VGame.Rectangle Rectangle = new Rectangle();
		public bool Visible = true;
		public bool IsCancel = false;
		protected bool selectable = true;

		public int Height = 22;
		public string Font = "04b_19";
		public string Font2 = "04b_19";
		public int TextSize = 20;
		public int TextSize2 = 20;
		public Vector2 TextOffset = Vector2.Zero;
		public Vector2 TextOffset2 = Vector2.Zero;
		public Color TextStroke = ColorPresets.Black;
		public Color TextStroke2 = ColorPresets.Black;
		public Color? BackgroundColor = null;

		public event EventHandler<EventArgs> Selected;
		public event EventHandler<EventArgs> SwipeLeft;
		public event EventHandler<EventArgs> SwipeRight;
		public event EventHandler<AsciiEventArgs> TextInput;
		public event EventHandler<TextChangeArgs> TextChanged;

		public MenuEntry(Menu menu, string text) {
			Menu = menu;
			Text = text;
		}

		protected internal virtual void OnSelectEntry() {
			if (Selected != null)
				Selected(this, new EventArgs());
		}
		public virtual void OnSwipeLeftEntry() {
			if (SwipeLeft != null)
				SwipeLeft(this, new EventArgs());
		}
		public virtual void OnSwipeRightEntry() {
			if (SwipeRight != null)
				SwipeRight(this, new EventArgs());
		}
		public virtual void OnTextInputEntry(List<char> ascii, bool backspace) {
			if (TextInput != null)
				TextInput(this, new AsciiEventArgs(ascii, backspace));
		}
		public virtual void OnTextChanged() {
			TextChanged(this, new TextChangeArgs(Text2));
		}
		public virtual void Update() {
		}
		public virtual void Draw(Vector2 origin, int width, float alpha) {
			Context g = Menu.Renderer.Context;
			Rectangle.X = (int)(origin.X - width / 2);
			Rectangle.Y = (int)origin.Y;
			Rectangle.Width = (int)width;
			Rectangle.Height = Height;
			if (BackgroundColor.HasValue) {
				g.MoveTo((origin + new Vector2(-(width / 2), 0)).ToPointD());
				g.LineTo((origin + new Vector2(width / 2, 0)).ToPointD());
				g.LineTo((origin + new Vector2(width / 2, Height)).ToPointD());
				g.LineTo((origin + new Vector2(-(width / 2), Height)).ToPointD());
				g.ClosePath();
				Util.StrokeAndFill(g, null, BackgroundColor);
			}
			if (Text2 != null) {
				Menu.Renderer.DrawText(origin + new Vector2(-4, Height / 2) + TextOffset, Text, TextSize, TextAlign.Right, TextAlign.Middle, FillColor, TextStroke, null, 0, Font);
				Menu.Renderer.DrawText(origin + new Vector2(4, Height / 2) + TextOffset2, Text2, TextSize2, TextAlign.Left, TextAlign.Middle, FillColor2, TextStroke2, null, 0, Font2);
			}
			else {
				Menu.Renderer.DrawText(origin + new Vector2(0, Height / 2) + TextOffset, Text, TextSize, TextAlign.Center, TextAlign.Middle, FillColor, TextStroke, null, 0, Font);
			}
		}
	}
	public class TextInputEntry : MenuEntry {
		public int CharacterLimit = 16;
		protected string Text2Last = "";
		public TextInputEntry(Menu menu, string text, string editableText) : base(menu, text) {
			Text2 = editableText;
			Text2Last = Text2;
			Font2 = "04b25";
			TextSize2 = 24;
			TextOffset2 = new Vector2(0, 0);
		}
		public override Cairo.Color FillColor {
			get {
				return Enabled ? LabelFillColor : DisabledFillColor;
			}
		}
		public override Cairo.Color FillColor2 {
			get {
				return base.FillColor;
			}
		}
		public virtual bool IsCharValid(char c) {
			return true;
		}
		public virtual bool IsStringValid(string s) {
			return true;
		}
		public override void OnTextInputEntry(List<char> ascii, bool backspace) {
			if (backspace && Text2.Length > 0) {
				Text2 = Text2.Substring(0, Text2.Length - 1);
			}
			if (ascii.Count > 0) {
				foreach (char c in ascii) {
					if (IsCharValid(c))
						Text2 += c;
				}
			}
			if (IsStringValid(Text2)) {
				if (Text2 != Text2Last) {
					OnTextChanged();
				}
				Text2Last = Text2;
			}
			else {
				Text2 = Text2Last;
			}
		}
	}
	public class NumberInputEntry : TextInputEntry {
		public int? Maximum = 49;
		public NumberInputEntry(Menu menu, string text, int number) : base(menu, text, number.ToString()) {
			TextChanged += delegate(object sender, TextChangeArgs e) {
				if (e.Text == "") {
					Text2 = "0";
					e.Text = "0";
				}
			};
		}
		public override void OnTextInputEntry(List<char> ascii, bool backspace) {
			if (Text2 == "0") {
				bool okay = true;
				if (ascii.Count > 0) {
					foreach (char c in ascii) {
						if (!IsCharValid(c))
							okay = false;
					}
				}
				if (okay)
					Text2 = "";
			}
			base.OnTextInputEntry(ascii, backspace);
		}
		public override bool IsCharValid(char c) {
			return char.IsDigit(c);
		}
		public override bool IsStringValid(string s) {
			if (Text2 == "")
				Text2 = "0";
			if (Maximum.HasValue) {
				if (int.Parse(Text2) > Maximum) {
					Text2 = Maximum.ToString();
				}
			}
			return true;
		}
	}
	public class AddressInputEntry : TextInputEntry {
		protected static Regex regex = new Regex(@"[a-zA-Z0-9\.\-]", RegexOptions.IgnoreCase);
		public AddressInputEntry(Menu menu, string text, string address) : base(menu, text, address) {
		}
		public override bool IsCharValid(char c) {
			return regex.IsMatch(c.ToString());
		}
	}
	public class SelectManyEntry : MenuEntry {
		public List<string> Options;
		public int SelectedIndex = 0;
		public SelectManyEntry(Menu menu, string text, List<string> options) : base(menu, text) {
			Options = options;
			SwipeLeft += delegate(object sender, EventArgs e) {
				SelectedIndex--;
				if (SelectedIndex < 0)
					SelectedIndex += Options.Count;
			};
			SwipeRight += delegate(object sender, EventArgs e) {
				SelectedIndex++;
				if (SelectedIndex >= Options.Count)
					SelectedIndex -= Options.Count;
			};
			Selected += delegate(object sender, EventArgs e) {
				SelectedIndex++;
				if (SelectedIndex >= Options.Count)
					SelectedIndex -= Options.Count;
			};
		}
		public override void Update() {
			Text2 = Options[SelectedIndex];
		}
	}
	public class HeadingEntry : MenuEntry {
		public static Cairo.Color HeadingColor = new Cairo.Color(0.65, 0.65, 0.65);
		public HeadingEntry(Menu menu, string text) : base(menu, text) {
			selectable = false;
			Font = "04b25";
			TextSize = 18;
			Height = 20;
		}
		public override Cairo.Color FillColor {
			get {
				return HeadingColor;
			}
		}
	}
	public class SpacerEntry : MenuEntry {
		public SpacerEntry(Menu menu) : base(menu, "") {
			Enabled = false;
			Height = Menu.Margin;
		}
	}
	public class CancelEntry : MenuEntry {
		public CancelEntry(Menu menu) : base(menu, "Cancel") {
			IsCancel = true;
		}
		public CancelEntry(Menu menu, string text) : base(menu, text) {
			IsCancel = true;
		}
	}
	public class AsciiEventArgs : EventArgs {
		public AsciiEventArgs(List<char> ascii, bool backspace) {
			Chars = ascii;
			Backspace = backspace;
		}
		public List<char> Chars;
		public bool Backspace;
	}
	public class TextChangeArgs : EventArgs {
		public TextChangeArgs(string text) {
			Text = text;
		}
		public string Text;
	}
}

