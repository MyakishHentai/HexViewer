using System;

namespace Cryptosoft.TypesGallery.BaseTypes.Data
{
	/// <summary>
	/// ���������� ������������ ��� ������� ���������� ����� � ��������� ��������
	/// </summary>
	public class PathNotResolvedException : Exception
	{
		internal PathNotResolvedException(BindingPath path):base(GetMessage(path))
		{

		}

		static string GetMessage(BindingPath path)
		{
			if (path.IsResolved)
				return "�������������� ������ ��� ���������� ��������";

			if (path.Object == null)
				return "�� ������ ������� ������ ��������";

			return "�� ������� ��������� �������� ��� " + path;
		}
	}
}