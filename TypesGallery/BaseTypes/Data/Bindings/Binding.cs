using System;
using System.Diagnostics;
using System.Globalization;
using Cryptosoft.TypesGallery.Events;

namespace Cryptosoft.TypesGallery.BaseTypes.Data
{
	/// <summary>
	/// ����� �������� ������
	/// </summary>
	/// <remarks>
	/// ������������:
	/// 1) ��������� ��� �������� �������� => �������� ���������� ���������� ��-��������� ��� �������� ��������
	/// 2) ��������� ��� ������� �������� => �������� ���������� ���������� ��-��������� �� ��������� ��������
	/// 3) �������� ������ => ������, ���������� � ���� ��������, ������������ �������� �� � �������� ������
	/// 4) ��� ��������  => ������, ���������� ������ ��������, ����������� ������� �� ������������� ������� �� �������������� ��������
	/// 5) ������ ��� ��������  =>  ������ ���������� ���������� ��� ������ �������� �� ��������� ������� �� �������������� ��������
	/// 6) �������� ������  => �������� ������, ����������� �� ����� ����� �������� ��������
	/// 
	/// ���������� ������ Binding():
	/// 1) ��� ������� Source � Target, ���������� ��������� ��������� ��� ���������� � ���������� ������� ��������
	/// 2) ��� BindingPath m_PathSourceProperty � m_PathTargetProperty �������� ������ ��� ��������� ����� ����������� �������
	/// 3) ��������� Converter, ����������� �������, ���� ������������ ���������� �� ����������� �����
	/// 4) ����������� Mode, ������������ �������� ���������� �������
	/// </remarks>
	public class Binding
	{
		#region �������������

		private readonly object m_SynchronizationObject = new object();

		private int m_LockCount;

		#endregion

		#region Converter

		/// <summary>
		/// ���������, �������������� ��� ��������
		/// </summary>
		public IValueConverter Converter { get; set; }

		/// <summary>
		/// ��������, �������������� ��� ������� ��� ��������������� ��������
		/// </summary>
		public object ConverterParameter { get; set; }

		/// <summary>
		/// ��������, �������������� ��� ��������������� ��������
		/// </summary>
		public CultureInfo ConverterCulture { get; set; }

		#endregion

		/// <summary>
		/// ������, �������� ���������� �� �������� �������� � ��������
		/// </summary>
		private readonly BindingPath m_PathSourceProperty;

		/// <summary>
		/// ������ ��������� ����������, ��������� � BindingPath � �������� ��������
		/// </summary>
		public Exception LastSourceException { get; private set; }

		/// <summary>
		/// ������, �������� ���������� � ������� �������� � ��������
		/// </summary>
		private readonly BindingPath m_PathTargetProperty;

		private bool m_TargetUpdated;
		private bool m_SourceUpdated;
		private bool m_Cleared;

		/// <summary>
		/// ��������, �������������� ��� ������ �������� (�������� ��� ������������ ��������� � �������� �����)
		/// </summary>
		public object FallbackValue { get; set; }

		/// <summary>
		/// ����������� ��������, �������������� ��� ��������, ��� ������� �������� �������� ������ �� �����
		/// </summary>
		public static object DoNothing { get; private set; }

		/// <summary>
		/// �������� ����� ��������
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
		/// ������� ����� ��������
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
		/// ������������� � �������� ��� �����������, ������������� � ��������
		/// </summary>
		public BindingMode Mode { get; set; }

		internal bool BindToDataSource { get; set; }

		internal bool BindToTarget { get; set; }

		/// <summary>
		/// ����������� �����������
		/// </summary>
		static Binding()
		{
			DoNothing = new object();
		}

		/// <summary>
		/// ����������� ������� ������ Binding ��� ������������� ��������
		/// </summary>
		/// <param name="targetProperty">���� � �������� ��������</param>
		/// <param name="sourceProperty">���� � ��������� ��������</param>
		/// <param name="mode">��� ����� ��-�</param>
		public Binding(string targetProperty, string sourceProperty, BindingMode mode = BindingMode.Default)
			: this(null, targetProperty, null, sourceProperty, mode)
		{
		}

		/// <summary>
		/// ����������� ������� ������ Binding
		/// </summary>
		/// <param name="target">������� (���������) ������</param>
		/// <param name="targetProperty">���� � �������� ��������</param>
		/// <param name="source">�������� (���������) ������</param>
		/// <param name="sourceProperty">���� � ��������� ��������</param>
		/// <param name="mode">��� ����� ��-�</param>
		public Binding(object target, string targetProperty, object source, string sourceProperty,
			BindingMode mode = BindingMode.Default)
		{
			if (string.IsNullOrWhiteSpace(targetProperty))
				throw new ArgumentException("���������� ������� ���� � �������� ��������");

			//��������������� ������� ��������������� �������� ��������
			//��������� ������ BindingPath
			//���������������� ��������� ����� � ��������� ����������
			//� ����� �������������� ����������, ����������� � ���� �����
			//������ ���� � ���������� ��-�� � �������� �������
			m_PathSourceProperty = new BindingPath(source, sourceProperty);

			//��������� �����: ���������� ���������� �������� ������� ��������� �� ��������� �������
			m_PathSourceProperty.Changed += (sender, args) => UpdateTargetInternal();

			//��������������� ������� ��������������� ������� ��������
			//��������� ������ BindingPath
			//���������������� ��������� ����� � ��������� ����������
			//� ����� �������������� ����������, ����������� � ���� �����
			//������ ���� �� ���������� ��-�� � �������� �������
			m_PathTargetProperty = new BindingPath(target, targetProperty);

			//��������� �����: ���������� ���������� ��������� ������� ��������� �� �������� �������
			m_PathTargetProperty.Changed += (sender, args) => UpdateSourceInternal();

			//��������� ���� ����� �������
			Mode = mode;
		}

		/// <summary>
		/// ���������� ����������� ��� �������� ����� �������� Binding
		/// </summary>
		/// <param name="binding">������, � �������� ���������� ��������</param>
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
		/// ��������� ������� ������ ��������� �� ��������� �������
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
		/// ���������� �������� �� ��������� ��� ���������� ���� ������
		/// </summary>
		/// <param name="type">���, ��� �������� ��������� �������� �������� �� ���������</param>
		/// <returns>������ ���� type �� ��������� �� ���������</returns>
		object GetDefaultValue(Type type)
		{
			if (type.IsValueType && Nullable.GetUnderlyingType(type) == null)
				return Activator.CreateInstance(type);

			return null;
		}

		/// <summary>
		/// ���������� �������� ������� ��������� ��������� (�� ���������� �� ����������)
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

					//��� ���������� ��������
					Type TargetType = m_PathTargetProperty.Type;

					//����:^ �������� �� ����������
					if (TargetType == null)
					{
						throw new PathNotResolvedException(m_PathTargetProperty);
					}

					//�������� ���������� ��������
					object Value = m_PathSourceProperty.Value;

					if (Value == DoNothing)
					{
					}
					else if (Converter != null)
					{
						Value = Converter.Convert(Value, TargetType, ConverterParameter, ConverterCulture);
					}
					//���� : ���� ��������� ������� ������� � ��� ����������
					else if (TargetType != m_PathSourceProperty.Type)
					{
						//������� ���������� � ������ �� ����������� �����
						Value = StandartTypesConverter(Value, TargetType);
					}

					if (Value == DoNothing)
					{
						Value = FallbackValue ?? GetDefaultValue(TargetType);
						//������� � �������������� �����
						//Debug.WriteLine("��� ���������� �������� �� ������� ������������� �������� {0} �� {1} � {2}.", Value, m_PathSourceProperty.Type, TargetType.Name);
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
		/// ��������� �������� ������ ��������� �� �������� �������
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
		/// ���������� ��������� ������� ��������� �������� (�� ���������� � ����������)
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


					// �������� �������� �� �������� ��������, ��������� � ������� ������ ���������,
					// ����������� �����������, ������������� �������� � ��������

					if (m_PathTargetProperty.Object == null)
						return;

					//��� ���������� ��������
					Type SourceType = m_PathSourceProperty.Type;
					if (SourceType == null)
						return;

					//���� : ���������� ��-�� �� ���������� � �� ��������
					if (!m_PathTargetProperty.IsResolved)
					{
						throw new PathNotResolvedException(m_PathTargetProperty);
					}

					try
					{
						LastSourceException = null;
						SetException();
						
						object Value = m_PathTargetProperty.Value;

						//���� : ��������������� �������� == null
						if (Value == DoNothing)
						{
						}
						//����:^ ��������� ������������������ ��������� �����
						else if (Converter != null)
						{
							Value = Converter.ConvertBack(Value, SourceType, ConverterParameter, ConverterCulture);
						}
						//���� : ���� ��������� ������� ������� � ��� ����������
						else if (SourceType != m_PathTargetProperty.Type)
						{
							//������� ���������� � ������ �� ����������� �����
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
		/// ����������� �������� �� ������ ������������ ���� � ������
		/// </summary>
		/// <param name="val">�������� ���������� ��������</param>
		/// <param name="targetType">��� ���������� ��������</param>
		/// <returns></returns>
		private static object StandartTypesConverter(object val, Type targetType)
		{
			if (val == null)
				return null;

			//���� : ���, � �������� ���������� �����������
			if (targetType.IsPrimitive)
			{
				val = Convert.ChangeType(val, targetType);
			}
			//���� : ���, � �������� ���������� ������
			else if (targetType == typeof(string))
			{
				val = val.ToString();
			}

			return val;
		}

		///// <summary>
		///// �������� ��� ����������� ����� ������� � �������� ��������
		///// </summary>
		///// <remarks>���� ���������� ��� ����������� �� ���������� => ���������� ����� �������������</remarks>
		//public static BindingMode GetBindingMode(Binding e)
		//{
		//	//���� ���� �� ������� � �������� �� ����������
		//	if (e.Target == null || e.Source == null)
		//	{
		//		//������� ��������������������� �����������
		//		return e.Mode;
		//	}

		//	//���� : ���� �� ��������� �������� ������������� ����� ������, � �� �������� �� ��������
		//	if (e.m_PathSourceProperty.Direct || e.m_PathTargetProperty.Direct)
		//	{
		//		//��������� �������� �� �������������
		//		return BindingMode.OneTime;
		//	}

		//	//���������-�� ��������� ������� ���������� INotifyPropertyChanged ��� INotifyCollectionChanged
		//	bool target = BindingPath.ChainBinding(e.m_PathTargetProperty);
		//	bool source = BindingPath.ChainBinding(e.m_PathSourceProperty);

		//	//���� : ��� ������� ��������� ���������
		//	if (target && source)
		//	{
		//		return BindingMode.TwoWay;
		//	}
		//	//���� : ������ ��������� (����������) ��-�� ������������ ��������� => ���������� �������� � �������� (����������) ��-��
		//	if (!target && source)
		//	{
		//		return BindingMode.OneWay;
		//	}
		//	//���� : ������ �������� (����������) ��-�� ������������ ��������� => ���������� �������� � ��������� (����������) ��-��
		//	if (target && !source)
		//	{
		//		return BindingMode.OneWayToSource;
		//	}
		//	//���� : ��� ������� �� ��������� ��������� => ���������� �������� (����������) ��-��
		//	return BindingMode.OneTime;
		//}

		/// <summary>
		/// ������� ������ Binding (�������� ��������)
		/// </summary>
		/// <remarks>������������ ����������� IClearable</remarks>
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
		/// ������� ����� ������� Binding � ���������� �������� � �������� ���������
		/// </summary>
		/// <param name="target">������� ������</param>
		/// <param name="source">�������� ������</param>
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
/// ��������� ������� ������ ��������� �� ��������� �������
/// </summary>
//public void UpdateTarget()
//{
//���� : ����������� ��������� ��������� ������� ������
//� ��� ������ ��������� ��������� INotifyPropertyChanged ��� INotifyCollectionChanged
//    if (CanTargetUpdate)
//    {
//        m_SynchronizationContext.Send(args => UpdateTargetCore(), null);
//    }
//}

//m_PathSourceProperty.Changed += (sender, args) => UpdateTarget();
//m_PathTargetProperty.Changed += (sender, args) => UpdateSource();

/// <summary>
/// ��������� �������� ������ ��������� �� �������� �������
/// </summary>
//public void UpdateSource()
//{
//���� : ����������� ��������� ��������� �������� ������
//� ��� ������ ��������� ��������� INotifyPropertyChanged ��� INotifyCollectionChanged
//    if (CanSourceUpdate)
//    {
//        m_SynchronizationContext.Send(args => UpdateSourceCore(), null);
//    }
//}