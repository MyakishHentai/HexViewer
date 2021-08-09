using System;

namespace Cryptosoft.TypesGallery.BaseTypes.Data
{
	/// <summary>
	/// ������������ �����, ������� ������������ ������� ICollectionView.CurrentChanging.
	/// </summary>
	/// <param name="sender">�������� �������.</param>
	/// <param name="args">���������� � �������.</param>
	public delegate void CurrentChangingEventHandler(object sender, CurrentChangingEventArgs args);

	/// <summary>
	/// �������� ���������� ��� ������� ICollectionView.CurrentChanging
	/// </summary>
	public class CurrentChangingEventArgs : EventArgs
	{
		/// <summary>
		/// �������������� ����� ��������� CurrentChangingEventArgs.
		/// </summary>
		public CurrentChangingEventArgs() : this(false) { }

		/// <summary>
		/// �������������� ����� ��������� CurrentChangingEventArgs � ��������� ��������� ���� IsCancelable.
		/// </summary>
		/// <param name="isCancelable">��������, ����������� �������� �� ������� ����������</param>
		public CurrentChangingEventArgs(bool isCancelable)
		{
			IsCancelable = isCancelable;
		}

		/// <summary>
		/// ���������� ��� ������������� ��������, ����������� �������� �� �������.
		/// </summary>
		/// <exception cref="InvalidOperationException">���� �������� CurrentChangingEventArgs.IsCancelable ����� �������� false.</exception>
		public bool Cancel { get; set; }

		/// <summary>
		/// ��������� ��������, �������� �� ������� ����������.
		/// </summary>
		public bool IsCancelable { get; private set; }
	}
}