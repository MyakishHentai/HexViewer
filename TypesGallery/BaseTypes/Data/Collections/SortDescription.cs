using System;
using System.Collections;
using System.ComponentModel;
//using Cryptosoft.BaseTypes;

namespace Cryptosoft.TypesGallery.BaseTypes.Data
{
	/// <summary>
	/// Объявляет направление и имя свойства, используемые для сортировки
	/// </summary>
	public struct SortDescription
	{
		private readonly ListSortDirection m_Direction;
		private readonly string m_PropertyName;

		/// <summary>
		/// Инициализирует экземпляр структуры SortDescription
		/// </summary>
		/// <param name="propertyName">Имя свойства по которому проводится сортировка.</param>
		/// <param name="direction">Направление сортировки.</param>
		/// <exception cref="ArgumentNullException">Параметр propertyName не может иметь значение null.</exception>
		/// <exception cref="ArgumentException">Параметр propertyName не может быть пустым.</exception>
		/// <exception cref="InvalidEnumArgumentException">Параметр direction имеет недопустимое значение.</exception>
		public SortDescription(string propertyName, ListSortDirection direction)
		{
			m_PropertyName = propertyName;
			m_Direction = direction;
		}

		/// <summary>
		/// Сравнивает два объекта SortDescription.
		/// </summary>
		/// <param name="sd1">Первый экземпляр для сравнения.</param>
		/// <param name="sd2">Второй экземпляр для сравнения.</param>
		/// <returns>true если два объекта не идентичны; иначе false.</returns>
		public static bool operator !=(SortDescription sd1, SortDescription sd2)
		{
			return !sd1.Equals(sd2);
		}

		/// <summary>
		/// Сравнивает два объекта SortDescription.
		/// </summary>
		/// <param name="sd1">Первый экземпляр для сравнения.</param>
		/// <param name="sd2">Второй экземпляр для сравнения.</param>
		/// <returns>true если два объекта идентичны; иначе false.</returns>
		public static bool operator ==(SortDescription sd1, SortDescription sd2)
		{
			return sd1.Equals(sd2);
		}

		/// <summary>
		/// Возвращает значение, указывающее напрвление сортировки
		/// </summary>
		public ListSortDirection Direction
		{
			get { return m_Direction; }
		}

		/// <summary>
		/// Возвращает имя свойства, используемое при сортировке
		/// </summary>
		public string PropertyName
		{
			get { return m_PropertyName; }
		}

		/// <summary>
		/// Сравнивает указанный экземпляр с текущим SortDescription
		/// </summary>
		/// <param name="obj">Экземпляр для сравнения</param>
		/// <returns>true если obj и текущий экземпляр имеют одно значение</returns>
		public override bool Equals(object obj)
		{
			return ((SortDescription) obj).m_Direction == m_Direction && string.Equals(((SortDescription) obj).m_PropertyName, m_PropertyName);
		}

		/// <summary>
		/// Возвращает хеш код для экземпляра структуры SortDescription
		/// </summary>
		/// <returns>Хеш код для данного экземпляра SortDescription</returns>
		public override int GetHashCode()
		{
			return MathOperations.GetHashCode(m_PropertyName, m_Direction);
		}
	}

	/// <summary>
	/// Объявляет направление и имя свойства, используемые для сортировки
	/// </summary>
	public abstract class SortDescriptionBase
	{
		private readonly ListSortDirection m_Direction;

		/// <summary>
		/// Возвращает значение, указывающее напрaвление сортировки
		/// </summary>
		public ListSortDirection Direction
		{
			get { return m_Direction; }
		}

		/// <summary>
		/// Инициализирует экземпляр класса SortDescriptionBase
		/// </summary>
		/// <param name="direction">Направление сортировки.</param>
		/// <exception cref="InvalidEnumArgumentException">Параметр direction имеет недопустимое значение.</exception>
		protected SortDescriptionBase(ListSortDirection direction)
		{
			m_Direction = direction;
		}

		/// <summary>
		/// Сравнивает два объекта SortDescriptionBase.
		/// </summary>
		/// <param name="sd1">Первый экземпляр для сравнения.</param>
		/// <param name="sd2">Второй экземпляр для сравнения.</param>
		/// <returns>true если два объекта не идентичны; иначе false.</returns>
		public static bool operator !=(SortDescriptionBase sd1, SortDescriptionBase sd2)
		{
			return !Equals(sd1, sd2);
		}

		/// <summary>
		/// Сравнивает два объекта SortDescriptionBase.
		/// </summary>
		/// <param name="sd1">Первый экземпляр для сравнения.</param>
		/// <param name="sd2">Второй экземпляр для сравнения.</param>
		/// <returns>true если два объекта идентичны; иначе false.</returns>
		public static bool operator ==(SortDescriptionBase sd1, SortDescriptionBase sd2)
		{
			return Equals(sd1, sd2);
		}

		/// <summary>
		/// Возвращает хеш код для экземпляра класса SortDescriptionBase
		/// </summary>
		/// <returns>Хеш код для данного экземпляра SortDescriptionBase</returns>
		public override int GetHashCode()
		{
			return MathOperations.GetHashCode(m_Direction);
		}

		/// <summary>
		/// Сравнивает указанный экземпляр с текущим SortDescriptionBase
		/// </summary>
		/// <param name="obj">Экземпляр для сравнения</param>
		/// <returns>true если obj и текущий экземпляр имеют одно значение</returns>
		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;

			return ((SortDescriptionBase)obj).m_Direction == m_Direction;
		}
	}

	/// <summary>
	/// Объявляет направление и имя свойства, используемые для сортировки по свойству
	/// </summary>
	public class PropertySortDescription : SortDescriptionBase
	{
		private readonly string m_PropertyName;

		/// <summary>
		/// Возвращает имя свойства, по которому проводится сортировка
		/// </summary>
		public string PropertyName
		{
			get { return m_PropertyName; }
		}

		/// <summary>
		/// Инициализирует экземпляр класса PropertySortDescription
		/// </summary>
		/// <param name="propertyName">Имя свойства, по которому проводится сортировка.</param>
		/// <param name="direction">Направление сортировки.</param>
		/// <exception cref="ArgumentNullException">Параметр propertyName не может иметь значение null.</exception>
		/// <exception cref="ArgumentException">Параметр propertyName не может быть пустым.</exception>
		/// <exception cref="InvalidEnumArgumentException">Параметр direction имеет недопустимое значение.</exception>
		public PropertySortDescription(string propertyName, ListSortDirection direction) : base(direction)
		{
			m_PropertyName = propertyName;
		}

		/// <summary>
		/// Возвращает хеш код для экземпляра класса PropertySortDescription
		/// </summary>
		/// <returns>Хеш код для данного экземпляра PropertySortDescription</returns>
		public override int GetHashCode()
		{
			return MathOperations.GetHashCode(m_PropertyName, Direction);
		}

		/// <summary>
		/// Сравнивает указанный экземпляр с текущим PropertySortDescription
		/// </summary>
		/// <param name="obj">Экземпляр для сравнения</param>
		/// <returns>true если obj и текущий экземпляр имеют одно значение</returns>
		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;

			return base.Equals(obj) && string.Equals(((PropertySortDescription)obj).m_PropertyName, m_PropertyName);
		}
	}

	/// <summary>
	/// Объявляет направление и компаратор, для пользовательской сортировки
	/// </summary>
	public class CustomSortDescription : SortDescriptionBase
	{
		private IComparer m_Comparer;

		public IComparer Comparer
		{
			get { return m_Comparer; }
		}

		public CustomSortDescription(IComparer comparer, ListSortDirection direction) : base(direction)
		{
			m_Comparer = comparer;
		}
	}
}