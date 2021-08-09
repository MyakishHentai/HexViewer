using System;
using System.Diagnostics;
using System.Globalization;
using Cryptosoft.TypesGallery.Events;

namespace Cryptosoft.TypesGallery.BaseTypes.Data
{
	/// <summary>
	/// Класс привязки данных
	/// </summary>
	/// <remarks>
	/// Терминалогия:
	/// 1) Первичное или исходное свойство => является источником информации по-умолчанию для целевого свойства
	/// 2) Вторичное или целевое свойство => является приемником информации по-умолчанию от исходного свойства
	/// 3) Корневой объект => объект, включающий в себя свойство, относительно которого он и является корнем
	/// 4) Имя свойства  => строка, содержащая уровни вложения, разделенные точками от определенного объекта до отслеживаемого свойства
	/// 5) Полное имя свойства  =>  строка ступенчато включающая все уровни вложения от исходного объекта до отслеживаемого свойства
	/// 6) Исходный объект  => корневой объект, находящийся на самом верху иерархии вложений
	/// 
	/// Устройство класса Binding():
	/// 1) Два объекта Source и Target, являющиеся исходными объектами для Первичного и Вторичного свойств привязки
	/// 2) Два BindingPath m_PathSourceProperty и m_PathTargetProperty хранящие полные или частичные имена связываемых свойств
	/// 3) Конвертер Converter, реализуемый вручную, если производится связывание не примитивных типов
	/// 4) Модификация Mode, определяющая характер связывания свойств
	/// </remarks>
	public class Binding
	{
		#region Синхронизация

		private readonly object m_SynchronizationObject = new object();

		private int m_LockCount;

		#endregion

		#region Converter

		/// <summary>
		/// Конвертер, использующийся при привязке
		/// </summary>
		public IValueConverter Converter { get; set; }

		/// <summary>
		/// Значение, использующееся как аргумет при конвертировании значений
		/// </summary>
		public object ConverterParameter { get; set; }

		/// <summary>
		/// Культура, использующаяся при конвертировании значений
		/// </summary>
		public CultureInfo ConverterCulture { get; set; }

		#endregion

		/// <summary>
		/// Объект, хранящий информацию об исходном свойстве в привязке
		/// </summary>
		private readonly BindingPath m_PathSourceProperty;

		/// <summary>
		/// Хранит последнее исключение, вызванное в BindingPath с исходным объектом
		/// </summary>
		public Exception LastSourceException { get; private set; }

		/// <summary>
		/// Объект, хранящий информацию о целевом свойстве в привязке
		/// </summary>
		private readonly BindingPath m_PathTargetProperty;

		private bool m_TargetUpdated;
		private bool m_SourceUpdated;
		private bool m_Cleared;

		/// <summary>
		/// Значение, использующееся при ошибке привязки (например при несовпадении исходного и целевого типов)
		/// </summary>
		public object FallbackValue { get; set; }

		/// <summary>
		/// Статическое значение, использующееся для указания, что никакие операции привязки делать не нужно
		/// </summary>
		public static object DoNothing { get; private set; }

		/// <summary>
		/// Исходная точка привязки
		/// </summary>
		public object Source
		{
			get
			{
				m_TargetUpdated = false;
				return m_PathSourceProperty.Object;
			}
			set { m_PathSourceProperty.Object = value; }
		}

		/// <summary>
		/// Целевая точка привязки
		/// </summary>
		public object Target
		{
			get { return m_PathTargetProperty.Object; }
			set
			{
				m_SourceUpdated = false;
				m_PathTargetProperty.Object = value;
			}
		}

		/// <summary>
		/// Устанавливает и сообщает тип модификации, установленной в привязке
		/// </summary>
		public BindingMode Mode { get; set; }

		internal bool BindToDataSource { get; set; }

		internal bool BindToTarget { get; set; }

		/// <summary>
		/// Статический конструктор
		/// </summary>
		static Binding()
		{
			DoNothing = new object();
		}

		/// <summary>
		/// Конструктор создает объект Binding без инициализации привязки
		/// </summary>
		/// <param name="targetProperty">Путь к целевому свойству</param>
		/// <param name="sourceProperty">Путь к исходному свойству</param>
		/// <param name="mode">Тип связи св-в</param>
		public Binding(string targetProperty, string sourceProperty, BindingMode mode = BindingMode.Default)
			: this(null, targetProperty, null, sourceProperty, mode)
		{
		}

		/// <summary>
		/// Конструктор создает объект Binding
		/// </summary>
		/// <param name="target">Целевой (вторичный) объект</param>
		/// <param name="targetProperty">Путь к целевому свойству</param>
		/// <param name="source">Исходный (первичный) объект</param>
		/// <param name="sourceProperty">Путь к исходному свойству</param>
		/// <param name="mode">Тип связи св-в</param>
		public Binding(object target, string targetProperty, object source, string sourceProperty,
			BindingMode mode = BindingMode.Default)
		{
			if (string.IsNullOrWhiteSpace(targetProperty))
				throw new ArgumentException("Необходимо указать путь к целевому свойству");

			//Конструирование объекта обрабатывающего исходное свойство
			//Создается объект BindingPath
			//Инициализируется анонимный метод с привязкой оповещения
			//В конце обрабатываются исключения, возникающие в этом блоке
			//Разбор пути к первичному св-ву и привязка события
			m_PathSourceProperty = new BindingPath(source, sourceProperty);

			//Анонимный метод: Разрешение обновления целевого объекта значением из исходного объекта
			m_PathSourceProperty.Changed += (sender, args) => UpdateTargetInternal();

			//Конструирование объекта обрабатывающего целевое свойство
			//Создается объект BindingPath
			//Инициализируется анонимный метод с привязкой оповещения
			//В конце обрабатываются исключения, возникающие в этом блоке
			//Разбор пути ко вторичному св-ву и привязка события
			m_PathTargetProperty = new BindingPath(target, targetProperty);

			//Анонимный метод: Разрешение обновления исходного объекта значением из целевого объекта
			m_PathTargetProperty.Changed += (sender, args) => UpdateSourceInternal();

			//Установка типа связи свойств
			Mode = mode;
		}

		/// <summary>
		/// Внутренний конструктор для создания копий объектов Binding
		/// </summary>
		/// <param name="binding">Объект, с которого копируются свойства</param>
		private Binding(Binding binding)
		{
			m_PathSourceProperty = binding.m_PathSourceProperty.Clone();
			m_PathSourceProperty.Changed += (sender, args) => UpdateTargetInternal();

			m_PathTargetProperty = binding.m_PathTargetProperty.Clone();
			m_PathTargetProperty.Changed += (sender, args) => UpdateSourceInternal();

			FallbackValue = binding.FallbackValue;
			Converter = binding.Converter;
			ConverterParameter = binding.ConverterParameter;
			ConverterCulture = binding.ConverterCulture;

			Mode = binding.Mode;
		}

		/// <summary>
		/// Обновляет целевой объект значением из исходного объекта
		/// </summary>
		public void UpdateTarget()
		{
			SynchronizationHelper.CurrentContext.Send(args => UpdateTargetCore(), null);
			m_TargetUpdated = true;
		}

		public void UpdateTargetInternal()
		{
			if (m_Cleared)
				return;

			if (Mode == BindingMode.OneWayToSource)
				return;

			if (Mode == BindingMode.OneTimeToSource)
				return;

			if (Mode == BindingMode.OneTime && m_TargetUpdated)
				return;

			UpdateTarget();
		}

		/// <summary>
		/// Возвращает значение по умолчанию для указанного типа данных
		/// </summary>
		/// <param name="type">Тип, для которого требуется получить значение по умолчанию</param>
		/// <returns>Объект типа type со значением по умолчанию</returns>
		object GetDefaultValue(Type type)
		{
			if (type.IsValueType && Nullable.GetUnderlyingType(type) == null)
				return Activator.CreateInstance(type);

			return null;
		}

		/// <summary>
		/// Обновление целевого объекта значением исходного (от первичного ко вторичному)
		/// </summary>
		private void UpdateTargetCore()
		{
			lock (m_SynchronizationObject)
			{
				if (m_Cleared)
					return;

				if (m_LockCount > 0)
					return;

				try
				{
					m_LockCount++;

					//Тип вторичного свойства
					Type TargetType = m_PathTargetProperty.Type;

					//Если:^ свойство не определено
					if (TargetType == null)
					{
						throw new PathNotResolvedException(m_PathTargetProperty);
					}

					//Значение первичного свойства
					object Value = m_PathSourceProperty.Value;

					if (Value == DoNothing)
					{
					}
					else if (Converter != null)
					{
						Value = Converter.Convert(Value, TargetType, ConverterParameter, ConverterCulture);
					}
					//Если : типы связанных свойств отличны и нет конвертера
					else if (TargetType != m_PathSourceProperty.Type)
					{
						//Попытка приведения к одному из стандартных типов
						Value = StandartTypesConverter(Value, TargetType);
					}

					if (Value == DoNothing)
					{
						Value = FallbackValue ?? GetDefaultValue(TargetType);
						//Неудача в преобразовании типов
						//Debug.WriteLine("При обновлении привязки не удалось преобразовать значение {0} из {1} в {2}.", Value, m_PathSourceProperty.Type, TargetType.Name);
					}

					m_PathTargetProperty.Value = Value;
				}
				finally
				{
					m_LockCount--;
				}
			}
		}

		/// <summary>
		/// Обновляет исходный объект значением из целевого объекта
		/// </summary>
		public void UpdateSource()
		{
			SynchronizationHelper.CurrentContext.Send(args => UpdateSourceCore(), null);
			m_SourceUpdated = true;
		}

		void UpdateSourceInternal()
		{
			if (m_Cleared)
				return;

			if (Mode == BindingMode.OneWay)
				return;

			if (Mode == BindingMode.OneTime)
				return;

			if (Mode == BindingMode.OneTimeToSource && m_SourceUpdated)
				return;

			UpdateSource();
		}

		/// <summary>
		/// Обновление исходного объекта значением целевого (от вторичного к первичному)
		/// </summary>
		private void UpdateSourceCore()
		{
			lock (m_SynchronizationObject)
			{
				if (m_Cleared)
					return;

				if (m_LockCount > 0)
					return;

				try
				{
					m_LockCount++;


					// Получаем значение из целевого элемента, проверяем с помощью правил валидации,
					// преобразуем конвертером, устанавливаем значение в источник

					if (m_PathTargetProperty.Object == null)
						return;

					//Тип вторичного свойства
					Type SourceType = m_PathSourceProperty.Type;
					if (SourceType == null)
						return;

					//Если : изменяемое св-во НЕ существует и НЕ доступно
					if (!m_PathTargetProperty.IsResolved)
					{
						throw new PathNotResolvedException(m_PathTargetProperty);
					}

					try
					{
						LastSourceException = null;
						SetException();
						
						object Value = m_PathTargetProperty.Value;

						//Если : устанавливается значение == null
						if (Value == DoNothing)
						{
						}
						//Если:^ определен специализированный конвертер типов
						else if (Converter != null)
						{
							Value = Converter.ConvertBack(Value, SourceType, ConverterParameter, ConverterCulture);
						}
						//Если : типы связанных свойств отличны и нет конвертера
						else if (SourceType != m_PathTargetProperty.Type)
						{
							//Попытка приведения к одному из стандартных типов
							Value = StandartTypesConverter(Value, SourceType);
						}

						m_PathSourceProperty.Value = Value;
					}
					catch (Exception Ex)
					{
						LastSourceException = Ex;
						SetException();
						Debug.Write(Ex);
					}
				}
				finally
				{
					m_LockCount--;
				}
			}
		}

		private void SetException()
		{
			foreach (var value in m_PathTargetProperty)
			{
				var target = value as IExceptionValidation;

				if (target == null)
					continue;

				target.Exception = PrepareException(LastSourceException);
				break;
			}
		}

		private Exception PrepareException(Exception ex)
		{
			if (ex == null)
				return null;

			var Exception = ex;

			return PrepareException(Exception.InnerException) ?? Exception;
		}

		/// <summary>
		/// Преобразует значения из одного стандартного типа в другой
		/// </summary>
		/// <param name="val">Значение первичного свойства</param>
		/// <param name="targetType">Тип вторичного свойства</param>
		/// <returns></returns>
		private static object StandartTypesConverter(object val, Type targetType)
		{
			if (val == null)
				return null;

			//Если : тип, к которому приводится стандартный
			if (targetType.IsPrimitive)
			{
				val = Convert.ChangeType(val, targetType);
			}
			//Если : тип, к которому приводится строка
			else if (targetType == typeof(string))
			{
				val = val.ToString();
			}

			return val;
		}

		///// <summary>
		///// Сообщает тип действующей связи свойств в указаной привязки
		///// </summary>
		///// <remarks>Если определить тип модификации не получилось => возвращает ранее установленную</remarks>
		//public static BindingMode GetBindingMode(Binding e)
		//{
		//	//Если одно из свойств в привязке не определено
		//	if (e.Target == null || e.Source == null)
		//	{
		//		//Возврат первичноустановленной модификации
		//		return e.Mode;
		//	}

		//	//Если : один из связанных объектов привязывается через директ, а не напрямую по свойству
		//	if (e.m_PathSourceProperty.Direct || e.m_PathTargetProperty.Direct)
		//	{
		//		//Изменение свойства не отслеживается
		//		return BindingMode.OneTime;
		//	}

		//	//Реализуют-ли связанные объекты интерфейсы INotifyPropertyChanged или INotifyCollectionChanged
		//	bool target = BindingPath.ChainBinding(e.m_PathTargetProperty);
		//	bool source = BindingPath.ChainBinding(e.m_PathSourceProperty);

		//	//Если : оба объекта реализуют интерфейс
		//	if (target && source)
		//	{
		//		return BindingMode.TwoWay;
		//	}
		//	//Если : объект исходного (первичного) св-ва поддерживает интерфейс => постоянная привязка к целевому (вторичному) св-ву
		//	if (!target && source)
		//	{
		//		return BindingMode.OneWay;
		//	}
		//	//Если : объект целевого (вторичного) св-ва поддерживает интерфейс => постоянная привязка к исходному (первичному) св-ву
		//	if (target && !source)
		//	{
		//		return BindingMode.OneWayToSource;
		//	}
		//	//Если : оба объекта не реализуют интерфейс => обновление целевого (вторичного) св-ва
		//	return BindingMode.OneTime;
		//}

		/// <summary>
		/// Очищает объект Binding (отменяет привязку)
		/// </summary>
		/// <remarks>Предоставлен интерфейсом IClearable</remarks>
		public void Clear()
		{
			lock (m_SynchronizationObject)
			{
				m_Cleared = true;

				m_PathSourceProperty.Clear();
				m_PathTargetProperty.Clear();
			}
		}


		/// <summary>
		/// Создает копию объекта Binding с указанными исходным и целевыми объектами
		/// </summary>
		/// <param name="target">Целевой объект</param>
		/// <param name="source">Исходный объект</param>
		/// <returns></returns>
		public Binding Clone(object target, object source)
		{
			return new Binding(this) {Target = target, Source = source};
		}

		public Binding Clone(object target, bool bindToTarget, object source, bool bindToDataSource)
		{
			return new Binding(this) { BindToTarget = bindToTarget, BindToDataSource = bindToDataSource, Target = target, Source = source };
		}
	}

	public interface IExceptionValidation
	{
		Exception Exception { get; set; }
	}
}


/// <summary>
/// Обновляет целевой объект значением из исходного объекта
/// </summary>
//public void UpdateTarget()
//{
//Если : модификация позволяет обновлять целевой объект
//и сам объект реализует интерфейс INotifyPropertyChanged или INotifyCollectionChanged
//    if (CanTargetUpdate)
//    {
//        m_SynchronizationContext.Send(args => UpdateTargetCore(), null);
//    }
//}

//m_PathSourceProperty.Changed += (sender, args) => UpdateTarget();
//m_PathTargetProperty.Changed += (sender, args) => UpdateSource();

/// <summary>
/// Обновляет исходный объект значением из целевого объекта
/// </summary>
//public void UpdateSource()
//{
//Если : модификация позволяет обновлять исходный объект
//и сам объект реализует интерфейс INotifyPropertyChanged или INotifyCollectionChanged
//    if (CanSourceUpdate)
//    {
//        m_SynchronizationContext.Send(args => UpdateSourceCore(), null);
//    }
//}