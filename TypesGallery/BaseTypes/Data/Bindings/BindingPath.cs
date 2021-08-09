using System;
using System.Collections;
using System.Text;

namespace Cryptosoft.TypesGallery.BaseTypes.Data
{
	/// <summary>
	/// Класс для обработки пути к свойству, использующийся в механизме привязок. Используется для доступа к привязанному свойству и отслеживания изменений его значения
	/// </summary>
	/// <remarks>
	/// Используется объектами класса Binding() для создания и поддержания связи свойств
	/// Устройство класса BindingPath():
	/// 1) Содержит исходный объект Object, от которого строится связь всех объектов BindingPathPart()
	/// 2) FirstElement содержит исходный объект, которое находится наверху пути к отслеживаемому свойству
	/// 3) LastElement, содержит корневой объект (свойство исходного объекта описанного в FirstElement) для следующего вложения
	/// 4) Таким образом от FirstElement через несколько LastElement (зависит от количества уровней вложений) исходный объект связывается с отслеживаемым свойством
	/// </remarks>
	internal class BindingPath
	{
		private BindingPathElement m_FirstElement;
		private BindingPathElement m_LastElement;
		private readonly string m_Path;

		/// <summary>
		/// Событие возникает при изменении значения в свойстве, к которому осуществлена привязка
		/// </summary>
		public event EventHandler Changed;

		/// <summary>
		/// Объект находящийся в корне пути
		/// </summary>
		public object Object
		{
			get { return m_FirstElement.Object; }
			set { m_FirstElement.Object = value; }
		}

		/// <summary>
		/// Возвращает true, если свойство по указанному пути существует и к нему имеется доступ
		/// </summary>
		public bool IsResolved
		{
			get
			{
				var Last = m_LastElement;

				if (Last == null)
					return false;

				if (Last.Object == null)
					return false;

				return Last.IsGetMethodExists;
			}
		}

		/// <summary>
		/// Устанавливает или возвращает значение свойства по данному пути
		/// </summary>
		public object Value
		{
			get
			{
				var Last = m_LastElement;

				if (Last == null)
					return Binding.DoNothing;

				var Obj = Last.Object;

				if (Obj == null)
					return Binding.DoNothing;

				if (!Last.IsGetMethodExists)
					return Binding.DoNothing;

				return Last.GetValue(Obj);
			}

			set
			{
				var Last = m_LastElement;

				if (Last == null)
					return;

				var Obj = Last.Object;

				if (Obj == null)
					return;

				if (!Last.IsSetMethodExists)
					return;

				//Установка нового значения свойству связанного объекта
				Last.SetValue(Obj, value);
			}
		}

		/// <summary>
		/// Возвращает тип привязанного свойства
		/// </summary>
		public Type Type
		{
			get
			{
				var Last = m_LastElement;

				if (Last == null)
					return null;

				if (Last.Object == null)
					return null;

				if (!Last.IsGetMethodExists)
					return null;

				return Last.ValueType;
			}
		}

		/// <summary>
		/// Внутренний конструктор, использующийся для копирования пути
		/// </summary>
		/// <param name="bindingPath">Копируемый путь</param>
		private BindingPath(BindingPath bindingPath)
		{
			m_Path = bindingPath.m_Path;

			BindingPathElement Current = bindingPath.m_FirstElement;

			while (Current != null)
			{
				AddLast(Current.Clone());

				//Получение следующего звена цепочки вложений
				Current = Current.Child;
			}

			if (m_LastElement != null)
			{
				m_LastElement.Changed += (sender, args) => OnChanged();
			}
		}

		/// <summary>
		/// Конструктор инициализирует экземпляр BindingPath с указанным корневым объектом и путем к свойству
		/// </summary>
		/// <param name="obj">Корневой объект, полный или частичный путь к свойству</param>
		/// <param name="path">Имя свойства корневого объекта или полное имя свойства</param>
		public BindingPath(object obj, string path)
		{
			//Установка пути к связываемому св-ву
			m_Path = path;

			Parse(m_Path);

			//Привязка корневого объекта
			Object = obj;

			//Если : цепочка вложений определена
			if (m_LastElement != null)
			{
				m_LastElement.Changed += (sender, args) => OnChanged();
			}
		}

		/// <summary>
		/// Добавляет элемент пути
		/// </summary>
		/// <param name="element">Добавляемый элемент</param>
		private void AddLast(BindingPathElement element)
		{
			//Если : последнего уровня нет => назначаем верхний уровень 
			if (m_LastElement == null)
			{
				m_FirstElement = element;
			}
			//Связывание последнего уровня с добавляемым элементом как дочерний и родительский
			else
			{
				m_LastElement.Child = element;
				element.Parent = m_LastElement;
			}
			//Добавляемый элемент становится последним
			m_LastElement = element;
		}

		/// <summary>
		/// Разбирает строковый путь к привязываемому свойству
		/// </summary>
		private void Parse(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				AddLast(new BindingPathElement(path));

				return;
			}

			//Конструктор строки
			var Builder = new StringBuilder(256);

			//Адрес проверяемого символа
			var Offset = 0;

			//Пока: адрес символа в пределах длины строки
			while (Offset < path.Length)
			{
				Offset = SkipWhiteSpaces(path, Offset);

				if (Offset >= path.Length)
					break;

				if (char.IsLetter(path, Offset) || path[Offset] == '_')
				{
					Builder.Clear();

					while (Offset < path.Length)
					{
						if (!char.IsLetterOrDigit(path, Offset) && path[Offset] != '_')
							break;

						Builder.Append(path[Offset]);
						//Смещение номера символа в пути
						Offset++;
					}

					AddLast(new BindingPathProperty(Builder.ToString()));

					Offset = SkipWhiteSpaces(path, Offset);

					if (Offset < path.Length && path[Offset] == '.')
						Offset++;
				}
				//Если : вложенный уровень индексируется
				else if (path[Offset] == '[')
				{
					Builder.Clear();

					//Смещение на первый символ индексации
					Offset++;

					//Пока : не закончится ключ или адрес
					while (Offset < path.Length)
					{
						if (path[Offset] == ']')
							break;

						Builder.Append(path[Offset]);
						Offset++;
					}

					//Смещение с символа ']'
					if (Offset < path.Length && path[Offset] == ']')
						Offset++;
					else
						throw new InvalidPathException();

					//Привязка к элементу контейнера
					AddLast(new BindingPathIndexer(Builder.ToString()));

					Offset = SkipWhiteSpaces(path, Offset);

					if (Offset < path.Length && path[Offset] == '.')
						Offset++;
				}
				//Если в строке есть запрещенные для записи имен символы
				else
				{
					throw new InvalidPathException();
				}
			}
		}

		/// <summary>
		/// Вызывает событие Changed
		/// </summary>
		protected virtual void OnChanged()
		{
			var Handler = Changed;
			if (Handler != null) Handler(this, EventArgs.Empty);
		}

		/// <summary>
		/// Вспомогательная функция для пропуска пробелов при разборе пути к свойству
		/// </summary>
		/// <param name="path">Разбираемый путь</param>
		/// <param name="index">Текущий индекс</param>
		/// <returns>Индекс в разбираемой строке, после которого отсутсвуют пробелы</returns>
		private static int SkipWhiteSpaces(string path, int index)
		{
			while (index < path.Length && char.IsWhiteSpace(path, index))
				index++;

			return index;
		}

		///// <summary>
		///// Сообщает, сохраняется-ли на протяжении всех вложений реализация интерфейсов оповещения изменения компонентов
		///// </summary>
		///// <param name="chain">Путь привязки</param>
		///// <returns>Истина, если каждое звено цепи реализует INotifyPropertyChanged или INotifyCollectionChanged</returns>
		//internal static bool ChainBinding(BindingPath chain)
		//{
		//	bool answer = true;

		//	//Если : привязка необходима к корневому объекту 
		//	if (chain.m_LastElement.Direct)
		//	{
		//		return false;
		//	}

		//	//Звено цепи вложения объектов
		//	BindingPathElement link = chain.m_FirstElement;

		//	do
		//	{
		//		//Если : звено цепи вложений реализует интерфейс оповещения изменения содержимого
		//		if (link.Object is INotifyPropertyChanged || link.Object is INotifyCollectionChanged)
		//		{
		//			//Переход на следующее звено цепи
		//			link = link.Child;
		//			continue;
		//		}

		//		//Цепочка оповещения нарушена
		//		answer = false;
		//		break;
		//	}
		//		//Пока : не пройдена вся цепочка вложений объектов
		//	while (link != null);

		//	return answer;
		//}

		/// <summary>
		/// Обнуляет привязку к уровням вложения
		/// </summary>
		/// <remarks>Предоставлен интерфейсом IClearable</remarks>
		public void Clear()
		{
			m_FirstElement.Clear();
		}

		/// <summary>
		/// Создает копию текущего экземпляра
		/// </summary>
		/// <returns>Копия текущего экземпляра</returns>
		public BindingPath Clone()
		{
			return new BindingPath(this);
		}

		public IEnumerator GetEnumerator()
		{
			var element = m_FirstElement;

			while (element != null)
			{
				yield return element.Object;
				element = element.Child;
			}
		}

		public override string ToString()
		{
			return "{" + Object + "}." + m_Path;
		}
	}
}