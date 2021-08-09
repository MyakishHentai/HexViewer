using System;
using System.Runtime.Serialization;

namespace Cryptosoft.TypesGallery.BaseTypes.Data
{
	/// <summary>
	/// ���������� ������������ ��� ������� ������� ����� � ��������� ��������
	/// </summary>
	[Serializable]
	public class InvalidPathException : Exception
	{
		/// <summary>
		/// ����������� ��-���������, ��������� �� ��� ������ �� ��������� � ������������� ��������� � ���������
		/// </summary>
		public InvalidPathException() : base("���� � �������� ����� �������� ������ ��� �������� ������������ �������")
		{
		}

		public InvalidPathException(string message) : base(message)
		{
		}

		public InvalidPathException(string message, Exception inner) : base(message, inner)
		{
		}

		protected InvalidPathException(
			SerializationInfo info,
			StreamingContext context) : base(info, context)
		{
		}
	}
}