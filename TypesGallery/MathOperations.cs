using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Cryptosoft.TypesGallery
{
	public static class MathOperations
	{
		/// <summary>
		/// Циклический сдвиг влево
		/// </summary>
		/// <param name="value">Сдвигаемое значение</param>
		/// <param name="shift">Сдвиг</param>
		/// <returns>Сдвинутое значение</returns>
		public static int CyclicLeftShift(this int value, int shift)
		{
			shift = shift % 0x20;

			return (value << shift) | (value >> (0x20 - shift));
		}

		/// <summary>
		/// Циклический сдвиг влево
		/// </summary>
		/// <param name="value">Сдвигаемое значение</param>
		/// <param name="shift">Сдвиг</param>
		/// <returns>Сдвинутое значение</returns>
		public static long CyclicLeftShift(this long value, int shift)
		{
			shift = shift % 0x40;

			return (value << shift) | (value >> (0x40 - shift));
		}

		/// <summary>
		/// Циклический сдвиг вправо
		/// </summary>
		/// <param name="value">Сдвигаемое значение</param>
		/// <param name="shift">Сдвиг</param>
		/// <returns>Сдвинутое значение</returns>
		public static int CyclicRightShift(this int value, int shift)
		{
			shift = shift % 0x20;

			return (value >> shift) | (value << (0x20 - shift));
		}

		/// <summary>
		/// Циклический сдвиг вправо
		/// </summary>
		/// <param name="value">Сдвигаемое значение</param>
		/// <param name="shift">Сдвиг</param>
		/// <returns>Сдвинутое значение</returns>
		public static long CyclicRightShift(this long value, int shift)
		{
			shift = shift % 0x40;

			return (value >> shift) | (value << (0x40 - shift));
		}

		/// <summary>
		/// Функция вычисляет хеш код для набора объектов
		/// </summary>
		/// <param name="firstObject"></param>
		/// <param name="objects"></param>
		/// <returns></returns>
		public static int GetHashCode(object firstObject, params object[] objects)
		{
			return GetHashCodeSeed(firstObject.GetHashCode(), objects);
		}

		public static int GetHashCodeSeed(int seed, params object[] objects)
		{
			var Hash = seed;

			return objects.Where(o => o != null).Aggregate(Hash, (current, o) => current.CyclicLeftShift(5) ^ o.GetHashCode());
		}

		public static int GetHashCode(IEnumerable objects)
		{
			return objects.Cast<object>().Aggregate(0, (current, o) => current ^ o.GetHashCode().CyclicLeftShift(o.GetType().GetHashCode()));
		}

	}
}
