using System;
using System.Collections;
using System.Text;

namespace Cryptosoft.TypesGallery.BaseTypes.Data
{
	/// <summary>
	/// ����� ��� ��������� ���� � ��������, �������������� � ��������� ��������. ������������ ��� ������� � ������������ �������� � ������������ ��������� ��� ��������
	/// </summary>
	/// <remarks>
	/// ������������ ��������� ������ Binding() ��� �������� � ����������� ����� �������
	/// ���������� ������ BindingPath():
	/// 1) �������� �������� ������ Object, �� �������� �������� ����� ���� �������� BindingPathPart()
	/// 2) FirstElement �������� �������� ������, ������� ��������� ������� ���� � �������������� ��������
	/// 3) LastElement, �������� �������� ������ (�������� ��������� ������� ���������� � FirstElement) ��� ���������� ��������
	/// 4) ����� ������� �� FirstElement ����� ��������� LastElement (������� �� ���������� ������� ��������) �������� ������ ����������� � ������������� ���������
	/// </remarks>
	internal class BindingPath
	{
		private BindingPathElement m_FirstElement;
		private BindingPathElement m_LastElement;
		private readonly string m_Path;

		/// <summary>
		/// ������� ��������� ��� ��������� �������� � ��������, � �������� ������������ ��������
		/// </summary>
		public event EventHandler Changed;

		/// <summary>
		/// ������ ����������� � ����� ����
		/// </summary>
		public object Object
		{
			get { return m_FirstElement.Object; }
			set { m_FirstElement.Object = value; }
		}

		/// <summary>
		/// ���������� true, ���� �������� �� ���������� ���� ���������� � � ���� ������� ������
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
		/// ������������� ��� ���������� �������� �������� �� ������� ����
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

				//��������� ������ �������� �������� ���������� �������
				Last.SetValue(Obj, value);
			}
		}

		/// <summary>
		/// ���������� ��� ������������ ��������
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
		/// ���������� �����������, �������������� ��� ����������� ����
		/// </summary>
		/// <param name="bindingPath">���������� ����</param>
		private BindingPath(BindingPath bindingPath)
		{
			m_Path = bindingPath.m_Path;

			BindingPathElement Current = bindingPath.m_FirstElement;

			while (Current != null)
			{
				AddLast(Current.Clone());

				//��������� ���������� ����� ������� ��������
				Current = Current.Child;
			}

			if (m_LastElement != null)
			{
				m_LastElement.Changed += (sender, args) => OnChanged();
			}
		}

		/// <summary>
		/// ����������� �������������� ��������� BindingPath � ��������� �������� �������� � ����� � ��������
		/// </summary>
		/// <param name="obj">�������� ������, ������ ��� ��������� ���� � ��������</param>
		/// <param name="path">��� �������� ��������� ������� ��� ������ ��� ��������</param>
		public BindingPath(object obj, string path)
		{
			//��������� ���� � ������������ ��-��
			m_Path = path;

			Parse(m_Path);

			//�������� ��������� �������
			Object = obj;

			//���� : ������� �������� ����������
			if (m_LastElement != null)
			{
				m_LastElement.Changed += (sender, args) => OnChanged();
			}
		}

		/// <summary>
		/// ��������� ������� ����
		/// </summary>
		/// <param name="element">����������� �������</param>
		private void AddLast(BindingPathElement element)
		{
			//���� : ���������� ������ ��� => ��������� ������� ������� 
			if (m_LastElement == null)
			{
				m_FirstElement = element;
			}
			//���������� ���������� ������ � ����������� ��������� ��� �������� � ������������
			else
			{
				m_LastElement.Child = element;
				element.Parent = m_LastElement;
			}
			//����������� ������� ���������� ���������
			m_LastElement = element;
		}

		/// <summary>
		/// ��������� ��������� ���� � �������������� ��������
		/// </summary>
		private void Parse(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				AddLast(new BindingPathElement(path));

				return;
			}

			//����������� ������
			var Builder = new StringBuilder(256);

			//����� ������������ �������
			var Offset = 0;

			//����: ����� ������� � �������� ����� ������
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
						//�������� ������ ������� � ����
						Offset++;
					}

					AddLast(new BindingPathProperty(Builder.ToString()));

					Offset = SkipWhiteSpaces(path, Offset);

					if (Offset < path.Length && path[Offset] == '.')
						Offset++;
				}
				//���� : ��������� ������� �������������
				else if (path[Offset] == '[')
				{
					Builder.Clear();

					//�������� �� ������ ������ ����������
					Offset++;

					//���� : �� ���������� ���� ��� �����
					while (Offset < path.Length)
					{
						if (path[Offset] == ']')
							break;

						Builder.Append(path[Offset]);
						Offset++;
					}

					//�������� � ������� ']'
					if (Offset < path.Length && path[Offset] == ']')
						Offset++;
					else
						throw new InvalidPathException();

					//�������� � �������� ����������
					AddLast(new BindingPathIndexer(Builder.ToString()));

					Offset = SkipWhiteSpaces(path, Offset);

					if (Offset < path.Length && path[Offset] == '.')
						Offset++;
				}
				//���� � ������ ���� ����������� ��� ������ ���� �������
				else
				{
					throw new InvalidPathException();
				}
			}
		}

		/// <summary>
		/// �������� ������� Changed
		/// </summary>
		protected virtual void OnChanged()
		{
			var Handler = Changed;
			if (Handler != null) Handler(this, EventArgs.Empty);
		}

		/// <summary>
		/// ��������������� ������� ��� �������� �������� ��� ������� ���� � ��������
		/// </summary>
		/// <param name="path">����������� ����</param>
		/// <param name="index">������� ������</param>
		/// <returns>������ � ����������� ������, ����� �������� ���������� �������</returns>
		private static int SkipWhiteSpaces(string path, int index)
		{
			while (index < path.Length && char.IsWhiteSpace(path, index))
				index++;

			return index;
		}

		///// <summary>
		///// ��������, �����������-�� �� ���������� ���� �������� ���������� ����������� ���������� ��������� �����������
		///// </summary>
		///// <param name="chain">���� ��������</param>
		///// <returns>������, ���� ������ ����� ���� ��������� INotifyPropertyChanged ��� INotifyCollectionChanged</returns>
		//internal static bool ChainBinding(BindingPath chain)
		//{
		//	bool answer = true;

		//	//���� : �������� ���������� � ��������� ������� 
		//	if (chain.m_LastElement.Direct)
		//	{
		//		return false;
		//	}

		//	//����� ���� �������� ��������
		//	BindingPathElement link = chain.m_FirstElement;

		//	do
		//	{
		//		//���� : ����� ���� �������� ��������� ��������� ���������� ��������� �����������
		//		if (link.Object is INotifyPropertyChanged || link.Object is INotifyCollectionChanged)
		//		{
		//			//������� �� ��������� ����� ����
		//			link = link.Child;
		//			continue;
		//		}

		//		//������� ���������� ��������
		//		answer = false;
		//		break;
		//	}
		//		//���� : �� �������� ��� ������� �������� ��������
		//	while (link != null);

		//	return answer;
		//}

		/// <summary>
		/// �������� �������� � ������� ��������
		/// </summary>
		/// <remarks>������������ ����������� IClearable</remarks>
		public void Clear()
		{
			m_FirstElement.Clear();
		}

		/// <summary>
		/// ������� ����� �������� ����������
		/// </summary>
		/// <returns>����� �������� ����������</returns>
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