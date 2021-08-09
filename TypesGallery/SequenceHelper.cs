using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cryptosoft.TypesGallery
{
	public static class SequenceHelper
	{
		public static void SetValues<T>(this IList<T> e, T value)
		{
			for (int i = 0; i < e.Count; i++)
				e[i] = value;

		}

		public static Byte[] StringToByteArray(String str, String seperator = " ")
		{
			String[] Bytes = str.Split(new String[] { seperator }, StringSplitOptions.None);
			var Result = new Byte[Bytes.Length];

			for (int i = 0; i < Bytes.Length; i++)
			{
				Result[i] = Byte.Parse(Bytes[i], System.Globalization.NumberStyles.HexNumber);
			}

			return Result;
		}

		public static String ByteArrayToString(Byte[] array, String seperator = " ")
		{
			StringBuilder Result = new StringBuilder();

			foreach (Byte Byte in array)
				Result.Append(Byte.ToString("x2") + seperator);

			// обрежем пробел в конце
			Result.Remove(Result.Length - 1, 1);

			return Result.ToString();
		}

		static public Boolean CompareArrays(Byte[] a1, Byte[] a2)
		{
			if (a1.Length != a2.Length)
				return false;

			for (int i = 0; i < a1.Length; i++)
				if (a1[i] != a2[i])
					return false;

			return true;
		}

		public static String PrintSequence<T>(this IEnumerable<T> sequence)
		{
			return PrintSequence(sequence, ", ", ".");
		}

		public static String PrintSequence<T>(this IEnumerable<T> sequence, String separator, String termination)
		{
			return PrintSequence<T>(sequence, separator, termination, null);
		}

		public static String PrintSequence<T>(this IEnumerable<T> sequence, String separator, String termination,
			Func<T, String> formatter)
		{
			var Result = new StringBuilder();
			Boolean First = true;

			foreach (T Element in sequence)
			{
				if (!First)
				{
					Result.Append(separator);
				}
				First = false;

				if (formatter == null)
				{
					Result.Append(Element.ToString());
				}
				else
				{
					Result.Append(formatter(Element));
				}
			}
			Result.Append(termination);

			return Result.ToString();
		}

		public static void ForEach<T>(this IEnumerable<T> source, Action<T> predicate)
		{
			foreach (T element in source)
				predicate.Invoke(element);
		}

		public static void ForEach<T>(this IEnumerable source, Action<T> predicate)
		{
			foreach (object element in source)
				predicate.Invoke((T)element);
		}

		public static void DisposeAll(this IEnumerable<IDisposable> source)
		{
			foreach (IDisposable element in source)
				element.Dispose();
		}

		public static T MaxBy<T, K>(this IEnumerable<T> source, Func<T, K> predicate) where K : IComparable
		{
			if (source == null || predicate == null)
				throw new ArgumentNullException();

			if (source.Count() == 0)
				throw new ArgumentException("source is empty!");

			T Max = source.First();
			K MaxK = predicate(Max);

			foreach (T Element in source)
			{
				var KValue = predicate(Element);

				if (KValue.CompareTo(MaxK) > 0)
				{
					Max = Element;
					MaxK = KValue;
				}
			}

			return Max;
		}

		static public T FirstOrDefault<T>(this IEnumerable source, Func<T, bool> predicate)
		{
			foreach (T item in source)
			{
				if (predicate(item))
				{
					return item;
				}
			}

			return default(T);
		}

		/// <summary>
		/// Вызывается, когда маршаллингом была получена последовательность char, но не все из них могут быть значимыми
		/// </summary>
		static public string MarshallCharArrayToString(char[] chars)
		{
			var resultStr = new String(chars);

			var index = resultStr.IndexOf('\0');

			return index == -1 ? resultStr : resultStr.Substring(0, index);
		}
	}
}
