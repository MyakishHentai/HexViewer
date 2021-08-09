using System;
using System.Collections.Specialized;
using System.Reflection;
using Cryptosoft.TypesGallery.Events;

namespace Cryptosoft.TypesGallery.BaseTypes.Data
{
	/// <summary>
	/// Составляющая часть пути к элементу контейнера в механизме привязок
	/// </summary>
	/// Является наследником от класса BindingPathPart() и используется для привязки свойств элементов коллекции
	/// Устройство класса BindingPathPartElement():
	/// 1) Содержит ключ или адрес элемента в коллекции, свойство которого описывает в m_Object.Target
	internal class BindingPathIndexer : BindingPathElement
	{
		private readonly NotifyCollectionChangedEventHandler m_Handler;

		/// <summary>
		/// Метод, возвращающий связанный элемент коллекции
		/// </summary>
		private MethodInfo m_GetMethod;

		/// <summary>
		/// Метод, устанавливающий связанный элемент коллекции
		/// </summary>
		private MethodInfo m_SetMethod;

		/// <summary>
		/// Индекс элемента в коллекции
		/// </summary>
		public object Index { get; private set; }

		/// <summary>
		/// Конструктор, с индексом элемента контейнера свойства
		/// </summary>
		/// <param name="index">Индекс элемента в контейнере для привязки</param>
		public BindingPathIndexer(string index) : base(index)
		{
			int IndexValue;

			if (int.TryParse(index, out IndexValue))
			{
				Index = IndexValue;
			}
			else
			{
				Index = index;
			}

			m_Handler = NotifyTarget_CollectionChanged;
		}

		protected override void RemoveNotifyListener(object target)
		{
			var NotifyTarget = target as INotifyCollectionChanged;

			//Если : старый объект реализует интерфейс его нужно отвязать
			if (NotifyTarget != null)
			{
				//Удаление старой привязки к событию
				CollectionChangedEventManager.RemoveListener(NotifyTarget, m_Handler);
			}
		}

		protected override void AddNotifyListener(object target)
		{
			//Реализует-ли привязанный контейнер интерфейс INotifyCollectionChanged
			var NotifyTarget = target as INotifyCollectionChanged;

			if (NotifyTarget != null)
			{
				CollectionChangedEventManager.AddListener(NotifyTarget, m_Handler);
			}
		}

		protected override void UpdateGetAndSetMethods()
		{
			//Если : при создании привязки было указано св-во привязки
			//а объект реализует интерфейс INotifyCollectionChanged 
			if (Index != null)
			{
				//Если : объект является массивом Array
				if (ObjectType.IsArray)
				{
					//Получение метода Array.GetValue
					m_GetMethod = ObjectType.GetMethod("Get", new[] {Index.GetType()});
					m_SetMethod = ObjectType.GetMethod("Set");
				}
				//объект любая другая коллекция
				else
				{
					//Получение элемента коллекции по индексу или ключу
					m_GetMethod = ObjectType.GetMethod("get_Item", new[] {Index.GetType()});
					m_SetMethod = ObjectType.GetMethod("set_Item");
				}
			}
			else
			{
				m_GetMethod = null;
				m_SetMethod = null;
			}
		}

		public override object GetValue(object obj)
		{
			return m_GetMethod == null ? null : m_GetMethod.Invoke(obj, new[] {Index});
		}

		public override void SetValue(object obj, object value)
		{
			//Если : объект наследника массив ищется метод SetValue(новое значение, индекс)
			//if (obj is Array)
			//{
				//Вызов найденного метода
				//m_SetMethod.Invoke(obj, new[] {value, Index});
			//}
			//Если : объект наследника коллекция, то ищется метод set_Item(индекс, новое значение)
			//else
			{
				//Вызов найденного метода
				m_SetMethod.Invoke(obj, new[] {Index, value});
			}
		}

		public override bool IsGetMethodExists
		{
			get { return m_GetMethod != null; }
		}

		public override bool IsSetMethodExists
		{
			get { return m_SetMethod != null; }
		}

		public override Type ValueType
		{
			get { return m_GetMethod.ReturnType; }
		}

		/// <summary>
		/// Обработчик события
		/// </summary>
		/// <param name="sender">Объект</param>
		/// <param name="e">Событие</param>
		private void NotifyTarget_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			UpdateObjects(sender);
		}

		public override BindingPathElement Clone()
		{
			return new BindingPathIndexer(PathValue);
		}
	}
}