using System;
using System.ComponentModel;
using System.Reflection;
using Cryptosoft.TypesGallery.Events;

namespace Cryptosoft.TypesGallery.BaseTypes.Data
{
	/// <summary>
	/// Элемент пути к свойству в механизме привязок. Представляет свойство с именем.
	/// </summary>
	/// <remarks>
	/// Используется объектами класса BindingPath для доступа к свойствам и отслеживания изменений их значений
	/// </remarks>
	internal class BindingPathProperty : BindingPathElement
	{
		// Устройство класса BindingPathPart():
		// 1) Содержит Родителя BindingPathPart Parent, корневой объект, свойство которого и описывает данный экземпляр
		// 2) Содержит Потомка BindingPathPart Child, относительно которого данный экземпляр является родителем
		// 3) Является звеном цепи от верхнего уровня иерархии до нижнего уровня представляющего отслеживаемое свойство

		#region Внутренние поля

		private readonly PropertyChangedEventHandler m_Handler;

		#endregion

		#region Внешние свойства

		/// <summary>
		/// Возвращает или задает имя отслеживаемого свойства
		/// </summary>
		public string PropertyName { get { return PathValue; } }


		/// <summary>
		/// Информация об отслеживаемом свойстве
		/// </summary>
		protected PropertyInfo Property { get; private set; }

		#endregion

		protected override void RemoveNotifyListener(object target)
		{
			var NotifyTarget = target as INotifyPropertyChanged;

			// Если : привязывается уже связанное сво-во
			if (NotifyTarget != null)
			{
				//Удаление старой привязки к событию
				PropertyChangedEventManager.RemoveListener(NotifyTarget, m_Handler);
			}
		}

		protected override void AddNotifyListener(object target)
		{
			var NotifyTarget = target as INotifyPropertyChanged;

			if (NotifyTarget != null)
			{
				PropertyChangedEventManager.AddListener(NotifyTarget, m_Handler);
			}
		}

		protected override void UpdateGetAndSetMethods()
		{
			Property = PropertyName == null ? null : ObjectType.GetProperty(PropertyName, BindingFlags.Instance | BindingFlags.Public);
		}

		public override object GetValue(object obj)
		{
			return Property == null ? null : Property.GetValue(obj);
		}

		public override void SetValue(object obj, object value)
		{
			if (Property == null)
				return;

			Property.SetValue(obj, value);
		}

		public override bool IsGetMethodExists
		{
			get { return Property != null; }
		}

		public override bool IsSetMethodExists
		{
			get { return Property != null && Property.SetMethod != null; }
		}


		public override Type ValueType
		{
			get { return Property.PropertyType; }
		}

		/// <summary>
		/// Конструктор инициализирует экземпляр BindingPathElement с указанным именем свойства
		/// </summary>
		/// <param name="propertyName">Имя свойства для привязки</param>
		public BindingPathProperty(string propertyName) : base(propertyName)
		{
			if (string.IsNullOrWhiteSpace(propertyName))
				throw new InvalidPathException();

			m_Handler = NotifyTarget_PropertyChanged;
		}

		/// <summary>
		/// Обработчик события изменения свойства
		/// </summary>
		/// <param name="sender">Инициатор события</param>
		/// <param name="e">Параметры события</param>
		private void NotifyTarget_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (!e.PropertyName.Equals(PropertyName))
				return;

			UpdateObjects(sender);
		}

		public override BindingPathElement Clone()
		{
			return new BindingPathProperty(PathValue);
		}
	}
}