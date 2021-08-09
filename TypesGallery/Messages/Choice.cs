using System;
using System.ComponentModel;

namespace Cryptosoft.TypesGallery
{
	/// <summary>
	/// Структура, устанавливающая наборы вариантов действий
	/// </summary>
	public struct Choice
	{
		#region Закрытые поля

		/// <summary>
		/// Текстовое представление объекта выбора
		/// </summary>
		private readonly string m_Text;

		/// <summary>
		/// Объект выбора
		/// </summary>
		private readonly object m_Value;

		/// <summary>
		/// Вариант по-умолчанию
		/// </summary>
		private static readonly Choice s_None = new Choice(null, null);

		/// <summary>
		/// Вариант подтверждения
		/// </summary>
		private static readonly Choice s_Ok = new Choice("Ок", MessageResult.Ok);

		/// <summary>
		/// Вариант отказа
		/// </summary>
		private static readonly Choice s_Cancel = new Choice("Отмена", MessageResult.Cancel);

		/// <summary>
		/// Удовлетворительный вариант
		/// </summary>
		private static readonly Choice s_Yes = new Choice("Да", MessageResult.Yes);

		/// <summary>
		/// Отрицательный вариант
		/// </summary>
		private static readonly Choice s_No = new Choice("Нет", MessageResult.No);

		/// <summary>
		/// Игнорирование выбора
		/// </summary>
		private static readonly Choice s_Ignore = new Choice("Игнорировать", MessageResult.Ignore);

		#endregion

		/// <summary>
		/// Вариант по-умолчанию
		/// </summary>
		public static Choice None
		{
			get { return s_None; }
		}

		/// <summary>
		/// Согласие
		/// </summary>
		public static Choice Ok
		{
			get { return s_Ok; }
		}

		/// <summary>
		/// Отмена
		/// </summary>
		public static Choice Cancel
		{
			get { return s_Cancel; }
		}

		/// <summary>
		/// Подтверждение
		/// </summary>
		public static Choice Yes
		{
			get { return s_Yes; }
		}

		/// <summary>
		/// Отрицание
		/// </summary>
		public static Choice No
		{
			get { return s_No; }
		}

		/// <summary>
		/// Игнорирование
		/// </summary>
		public static Choice Ignore
		{
			get { return s_Ignore; }
		}

		/// <summary>
		/// Пустой-ли выбор
		/// </summary>
		public bool IsEmpty
		{
			get { return Value == null; }
		}

		/// <summary>
		/// Объект структуры
		/// </summary>
		public object Value
		{
			get { return m_Value; }
		}

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="value">Объект</param>
		public Choice(object value)
		{
			m_Text = value != null ? value.ToString() : string.Empty;
			m_Value = value;
		}

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="text">Текст</param>
		/// <param name="value">Объект</param>
		public Choice(string text, object value)
		{
			m_Text = text;
			m_Value = value;
		}

		/// <summary>
		/// Перегрузка равенства двух выборов
		/// </summary>
		/// <param name="one">Первый выбор</param>
		/// <param name="two">Второй выбор</param>
		/// <returns>Результат сравнения внутренних значений</returns>
		public static bool operator ==(Choice one, Choice two)
		{
			return one.Value == two.Value;
		}

		/// <summary>
		/// Перегрузка НЕ равенства двух выборов
		/// </summary>
		/// <param name="one">Первый выбор</param>
		/// <param name="two">Второй выбор</param>
		/// <returns>Результат сравнения внутренних значений</returns>
		public static bool operator !=(Choice one, Choice two)
		{
			return one.Value != two.Value;
		}

		/// <summary>
		/// Текстовое представление структуры
		/// </summary>
		/// <returns>Текстовое содержание, описывающее внутренний объект</returns>
		public override string ToString()
		{
			return m_Text;
		}
	}

	[Flags]
	public enum ChoiceOptions
	{
		None = 0x0,
		Default = 0x1,
		Exit = 0x2
	}
}