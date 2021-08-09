using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace Cryptosoft.TypesGallery
{
	public class ChoiceCollection : IEnumerable<Choice>
	{
		static readonly ChoiceCollection s_Ok = new ChoiceCollection { { Choice.Ok, ChoiceOptions.Default | ChoiceOptions.Exit } };
		static readonly ChoiceCollection s_YesNoCancel = new ChoiceCollection { { Choice.Yes, ChoiceOptions.Default }, Choice.No, { Choice.Cancel, ChoiceOptions.Exit } };
		static readonly ChoiceCollection s_YesNo = new ChoiceCollection { { Choice.Yes, ChoiceOptions.Default }, { Choice.No, ChoiceOptions.Exit } };

		public static ChoiceCollection Ok { get { return s_Ok; } }
		public static ChoiceCollection YesNoCancel { get { return s_YesNoCancel; } }
		public static ChoiceCollection YesNo { get { return s_YesNo; } }

		/// <summary>
		/// Список выборов
		/// </summary>
		private readonly List<Choice> m_Choices = new List<Choice>();

		public void Clear()
		{
			Default = Choice.None;
			Exit = Choice.None;
			m_Choices.Clear();
		}

		/// <summary>
		/// Индексация коллекции
		/// </summary>
		/// <param name="index">номер элемента коллекции</param>
		/// <returns></returns>
		public Choice this[int index]
		{
			get { return m_Choices[index]; }
		}

		/// <summary>
		/// Является-ли выбор приоритетным по-умолчанию
		/// </summary>
		public Choice Default { get; private set; }

		/// <summary>
		/// Исполняется-ли выбор при агрессивном выходе
		/// </summary>
		/// <remarks>Чаще всего агрессивных выход подразумевает нажатие кнопки ESC</remarks>
		public Choice Exit { get; private set; }

		/// <summary>
		/// Добавление выбора
		/// </summary>
		/// <param name="value">Объект в выборе</param>
		/// <param name="options"></param>
		public void Add(object value, ChoiceOptions options = ChoiceOptions.None)
		{
			Add(new Choice(value), options);
		}

		/// <summary>
		/// Добавление выбора
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value">Выбор</param>
		/// <param name="options"></param>
		public void Add(string name, Choice value, ChoiceOptions options = ChoiceOptions.None)
		{
			Add(new Choice(name, value.Value), options);
		}

		/// <summary>
		/// Добавление выбора
		/// </summary>
		/// <param name="name">Текст элемента выбора</param>
		/// <param name="value">Выбор</param>
		/// <param name="options"></param>
		public void Add(string name, object value, ChoiceOptions options = ChoiceOptions.None)
		{
			Add(new Choice(name, value), options);
		}

		/// <summary>
		/// Добавление элемента выбора
		/// </summary>
		/// <param name="choice">Элемент выбора</param>
		/// <param name="options">Параметры элемента</param>
		public void Add(Choice choice, ChoiceOptions options = ChoiceOptions.None)
		{
			m_Choices.Add(choice);

			// В зависимости от параметров устанавливаем значение выбора по умолчанию и значение выбора при отмене
			if ((options & ChoiceOptions.Default) != 0) Default = choice;

			if ((options & ChoiceOptions.Exit) != 0) Exit = choice;
		}

		public IEnumerator<Choice> GetEnumerator()
		{
			return m_Choices.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}