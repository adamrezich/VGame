using System;
using System.Collections.Generic;
using System.Reflection;
using System.ArrayExtensions;
using Cairo;

namespace VGame {
	public static class Extensions {
		public static PointD ToPointD(this Vector2 v) {
			return new PointD(v.X, v.Y);
		}

		public static Vector2 AddLengthDir(this Vector2 v, double length, double dir) {
			return v + new Vector2((float)(Math.Cos(dir) * length), (float)(Math.Sin(dir) * length));
		}

		public static double LerpAngle(this double from, double to, double step) {
			// Ensure that 0 <= angle < 2pi for both "from" and "to" 
			while (from < 0)
				from += MathHelper.TwoPi;
			while (from >= MathHelper.TwoPi)
				from -= MathHelper.TwoPi;

			while (to < 0)
				to += MathHelper.TwoPi;
			while (to >= MathHelper.TwoPi)
				to -= MathHelper.TwoPi;

			if (System.Math.Abs(from - to) < MathHelper.Pi) {
				// The simple case - a straight lerp will do. 
				return (double)MathHelper.Lerp((float)from, (float)to, (float)step);
			}

			// If we get here we have the more complex case. 
			// First, increment the lesser value to be greater. 
			if (from < to)
				from += MathHelper.TwoPi;
			else
				to += MathHelper.TwoPi;

			float retVal = MathHelper.Lerp((float)from, (float)to, (float)step);

			// Now ensure the return value is between 0 and 2pi 
			if (retVal >= MathHelper.TwoPi)
				retVal -= MathHelper.TwoPi;
			return retVal;
		}

		public static string MakeDecimal(this string str) {
			if (str.Length < 2 || str.Substring(str.Length - 2, 1) != ".") {
				str += ".0";
			}
			return str;
		}

		public static bool IntersectsLine(this VGame.Rectangle r, Vector2 p1, Vector2 p2) {
			return IntersectsLine(r, new Point((int)p1.X, (int)p1.Y), new Point((int)p2.X, (int)p2.Y));
		}

		public static bool IntersectsLine(this VGame.Rectangle r, Point p1, Point p2) {
			return LineIntersectsLine(p1, p2, new Point(r.X, r.Y), new Point(r.X + r.Width, r.Y)) ||
				LineIntersectsLine(p1, p2, new Point(r.X + r.Width, r.Y), new Point(r.X + r.Width, r.Y + r.Height)) ||
				LineIntersectsLine(p1, p2, new Point(r.X + r.Width, r.Y + r.Height), new Point(r.X, r.Y + r.Height)) ||
				LineIntersectsLine(p1, p2, new Point(r.X, r.Y + r.Height), new Point(r.X, r.Y)) ||
				(r.Contains(p1) && r.Contains(p2));
		}

		private static bool LineIntersectsLine(Point l1p1, Point l1p2, Point l2p1, Point l2p2) {
			float q = (l1p1.Y - l2p1.Y) * (l2p2.X - l2p1.X) - (l1p1.X - l2p1.X) * (l2p2.Y - l2p1.Y);
			float d = (l1p2.X - l1p1.X) * (l2p2.Y - l2p1.Y) - (l1p2.Y - l1p1.Y) * (l2p2.X - l2p1.X);

			if (d == 0) {
				return false;
			}

			float r = q / d;

			q = (l1p1.Y - l2p1.Y) * (l1p2.X - l1p1.X) - (l1p1.X - l2p1.X) * (l1p2.Y - l1p1.Y);
			float s = q / d;

			if (r < 0 || r > 1 || s < 0 || s > 1) {
				return false;
			}

			return true;
		}
	}
}
namespace System {
	public static class ObjectExtensions {
		private static readonly MethodInfo CloneMethod = typeof(Object).GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance);

		public static bool IsPrimitive(this Type type) {
			if (type == typeof(String))
				return true;
			return (type.IsValueType & type.IsPrimitive);
		}

		public static Object Copy(this Object originalObject) {
			return InternalCopy(originalObject, new Dictionary<Object, Object>(new ReferenceEqualityComparer()));
		}

		private static Object InternalCopy(Object originalObject, IDictionary<Object, Object> visited) {
			if (originalObject == null)
				return null;
			var typeToReflect = originalObject.GetType();
			if (IsPrimitive(typeToReflect))
				return originalObject;
			if (visited.ContainsKey(originalObject))
				return visited[originalObject];
			var cloneObject = CloneMethod.Invoke(originalObject, null);
			if (typeToReflect.IsArray) {
				var arrayType = typeToReflect.GetElementType();
				if (IsPrimitive(arrayType) == false) {
					Array clonedArray = (Array)cloneObject;
					clonedArray.ForEach((array, indices) => array.SetValue(InternalCopy(clonedArray.GetValue(indices), visited), indices));
				}

			}
			visited.Add(originalObject, cloneObject);
			CopyFields(originalObject, visited, cloneObject, typeToReflect);
			RecursiveCopyBaseTypePrivateFields(originalObject, visited, cloneObject, typeToReflect);
			return cloneObject;
		}

		private static void RecursiveCopyBaseTypePrivateFields(object originalObject, IDictionary<object, object> visited, object cloneObject, Type typeToReflect) {
			if (typeToReflect.BaseType != null) {
				RecursiveCopyBaseTypePrivateFields(originalObject, visited, cloneObject, typeToReflect.BaseType);
				CopyFields(originalObject, visited, cloneObject, typeToReflect.BaseType, BindingFlags.Instance | BindingFlags.NonPublic, info => info.IsPrivate);
			}
		}

		private static void CopyFields(object originalObject, IDictionary<object, object> visited, object cloneObject, Type typeToReflect, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy, Func<FieldInfo, bool> filter = null) {
			foreach (FieldInfo fieldInfo in typeToReflect.GetFields(bindingFlags)) {
				if (filter != null && filter(fieldInfo) == false)
					continue;
				if (IsPrimitive(fieldInfo.FieldType))
					continue;
				var originalFieldValue = fieldInfo.GetValue(originalObject);
				var clonedFieldValue = originalFieldValue == null ? null : InternalCopy(originalFieldValue, visited);
				fieldInfo.SetValue(cloneObject, clonedFieldValue);
			}
		}

		public static T Copy<T>(this T original) {
			return (T)Copy((Object)original);
		}
	}

	public class ReferenceEqualityComparer : EqualityComparer<Object> {
		public override bool Equals(object x, object y) {
			return ReferenceEquals(x, y);
		}

		public override int GetHashCode(object obj) {
			if (obj == null)
				return 0;
			return obj.GetHashCode();
		}
	}
	namespace ArrayExtensions {
		public static class ArrayExtensions {
			public static void ForEach(this Array array, Action<Array, int[]> action) {
				if (array.LongLength == 0)
					return;
				ArrayTraverse walker = new ArrayTraverse(array);
				do
					action(array, walker.Position); while (walker.Step());
			}
		}

		internal class ArrayTraverse {
			public int[] Position;
			private int[] maxLengths;

			public ArrayTraverse(Array array) {
				maxLengths = new int[array.Rank];
				for (int i = 0; i < array.Rank; ++i) {
					maxLengths[i] = array.GetLength(i) - 1;
				}
				Position = new int[array.Rank];
			}

			public bool Step() {
				for (int i = 0; i < Position.Length; ++i) {
					if (Position[i] < maxLengths[i]) {
						Position[i]++;
						for (int j = 0; j < i; j++) {
							Position[j] = 0;
						}
						return true;
					}
				}
				return false;
			}
		}
	}
}