using System;
using System.ComponentModel;
using System.Reflection;
using Cryptosoft.TypesGallery.Events;

namespace Cryptosoft.TypesGallery.BaseTypes.Data
{
	/// <summary>
	/// Элемент пути к свойству в механизме привязок
	/// </summary>
	internal class BindingPathElement
	{
		/// <summary>
		/// Слабая ссылка на объект, у которого отслеживается свойство
		/// </summary>
		protected readonly WeakReference m_Object = new WeakReference(null);

		/// <summary>
		/// Событие генерируется при изменении отслеживаемого свойства
		/// </summary>
		public event EventHandler Changed;


		public Type ObjectType { get; protected set; }

		protected string PathValue { get; private set; }

		/// <summary>
		/// Дочерний элемент пути
		/// </summary>
		public BindingPathElement Child { get; internal set; }

		/// <summary>
		/// Родительский элемент пути
		/// </summary>
		public BindingPathElement Parent { get; internal set; }

		/// <summary>
		/// Объект, чье свойство отслеживается
		/// </summary>
		public object Object
		{
			get { return m_Object.Target; }
			internal set
			{
				bool IsValueType = value != null && value.GetType().IsValueType;

				object Target = m_Object.Target;

				// Значение не изменилось
				if (Target == value)
					return;

				// Если : привязка обновляется
				if (Target != null)
				{
					RemoveNotifyListener(Target);
				}

				// Иначе : стандартное вложение связываемых уровней
				m_Object.Target = value;
				

				// Если указан нулл объект при биндинге
				if (value == null)
				{
					if (Child != null)
						Child.Object = null;
					else
						OnChanged();
				}
				// Если : корневой объект привязки был указан
				else
				{
					AddNotifyListener(value);

					UpdateObjects(value);
				}
			}
		}


		/// <summary>
		/// Конструктор инициализирует экземпляр BindingPathElement с указанным именем свойства
		/// </summary>
		/// <param name="pathValue">Имя свойства для привязки</param>
		public BindingPathElement(string pathValue = null)
		{
			PathValue = pathValue;
		}

		protected void UpdateObjects(object obj)
		{
			var NewObjectType = obj.GetType();

			if (NewObjectType != ObjectType)
			{
				ObjectType = NewObjectType;

				UpdateGetAndSetMethods();
			}

			// Если в пути есть элементы более низкого уровня, то рекурсивно обновляются отслеживаемые ими объекты
			if (Child != null)
			{
				// Если у объекта НЕТ доступного свойства с нужным именем, то обнуляем у дочернего элемента пути отслеживаемый объект,
				// иначе дочернему элементу пути назначается значение этого свойства в качестве отслеживаемого объекта
				Child.Object = GetValue(obj);
			}
			else
			{
				// Мы достигли нижнего уровня пути (нет дочернего элемента) - генерируем событие Changed
				OnChanged();
			}
		}

		protected virtual void RemoveNotifyListener(object target)
		{
		}

		protected virtual void AddNotifyListener(object target)
		{
		}

		protected virtual void UpdateGetAndSetMethods()
		{
		}

		public virtual object GetValue(object obj)
		{
			return obj;
		}

		public virtual void SetValue(object obj, object value)
		{
			throw new NotSupportedException();
		}

		public virtual bool IsGetMethodExists
		{
			get { return true; }
		}

		public virtual bool IsSetMethodExists
		{
			get { return false; }
		}

		public virtual Type ValueType
		{
			get { return ObjectType; }
		}

		/// <summary>
		/// Вызывает событие Changed
		/// </summary>
		protected void OnChanged()
		{
			var Handler = Changed;
			if (Handler != null) Handler(this, EventArgs.Empty);
		}

		public void Clear()
		{
			object Target = m_Object.Target;

			if (Target == null)
				return;

			RemoveNotifyListener(Target);

			m_Object.Target = null;

			if (Child != null)
				Child.Clear();
		}

		public virtual BindingPathElement Clone()
		{
			return new BindingPathElement(PathValue);
		}
	}
}