using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;

namespace Cryptosoft.TypesGallery.BaseTypes.Data
{
	/// <summary>
	/// ��������� �������������� ����������������� �������� ��������, ����������, ���������� � �����������
	/// </summary>
	public interface ICollectionView : IEnumerable, INotifyCollectionChanged
	{
		/// <summary>
		/// ��������, ����������� ��� ������ ����������� ������������ ���������� � ������� �������� ICollectionView.Filter
		/// </summary>
		/// <returns>true ���� ����������� ������������ ����������; �����, false.</returns>
		bool CanFilter { get; }

		/// <summary>
		/// ��������, ����������� ��� ������ ����������� ������������ ����������� � ������� �������� ICollectionView.GroupDescriptions
		/// </summary>
		/// <returns>true ���� ����������� ������������ �����������; �����, false.</returns>
		bool CanGroup { get; }

		/// <summary>
		/// ��������, ����������� ��� ������ ����������� ������������ ���������� � ������� �������� ICollectionView.SortDescriptions
		/// </summary>
		/// <returns>true ���� ����������� ������������ ����������; �����, false.</returns>
		bool CanSort { get; }

		/// <summary>
		/// ���������� ��� ������������� �������� ��� ��������, ������� ����� �������� �� ��������, �������� ����������.
		/// </summary>
		CultureInfo Culture { get; set; }

		/// <summary>
		/// ���������� ������� ������� � �����������
		/// </summary>
		/// <returns>������� ������� ��� null, ���� ��� �������� ��������.</returns>
		object CurrentItem { get; }

		/// <summary>
		/// ���������� ���������� ����� ICollectionView.CurrentItem � �����������.
		/// </summary>
		/// <returns>������� ICollectionView.CurrentItem � �����������.</returns>
		int CurrentPosition { get; }

		/// <summary>
		/// �������� ��� ������������� �������� �����, ������������ ��� ����������� ������� �� �������� ������� � �����������.
		/// </summary>
		/// <returns>�����, ������������ ��� ����������� ������� �� �������� ������� � �����������.</returns>
		Predicate<object> Filter { get; set; }

		/// <summary>
		/// ���������� ��������� �������� GroupDescription, ������� ��������� ��� ������������ �������� � �����������
		/// </summary>
		ObservableCollection<GroupDescription> GroupDescriptions { get; }

		/// <summary>
		/// ���������� ������
		/// </summary>
		/// <returns>��������� �����, ��������� ������ ��� ������ ��� null, ���� ����� ���.</returns>
		ReadOnlyObservableCollection<object> Groups { get; }

		/// <summary>
		/// ���������� ��������, ����������� ��� ICollectionView.CurrentItem �� ��������� ���������
		/// </summary>
		/// <returns>true ���� ICollectionView.CurrentItem �� ��������� ���������; ����� false</returns>
		bool IsCurrentAfterLast { get; }

		/// <summary>
		/// ���������� ��������, ����������� ��� ICollectionView.CurrentItem �� ��������� ���������
		/// </summary>
		/// <returns>true ���� ICollectionView.CurrentItem �� ��������� ���������; ����� false</returns>
		bool IsCurrentBeforeFirst { get; }

		/// <summary>
		/// ���������� ��������, ����������� �������� �� ����������� ������
		/// </summary>
		/// <returns>true ���� ����������� ������; ����� false.</returns>
		bool IsEmpty { get; }

		/// <summary>
		/// ���������� ��������� �������� SortDescription, ������������ ��� �������� ����������� � �����������.
		/// </summary>
		SortDescriptionCollection SortDescriptions { get; }

		/// <summary>
		/// ���������� ���������� ���������
		/// </summary>
		IEnumerable SourceCollection { get; }

		/// <summary>
		/// ��� ���������� ���������� �������� ��� ������� ����� ��������� �������� �������� ��������.
		/// </summary>
		event EventHandler CurrentChanged;

		///// <summary>
		///// ��� ���������� ���������� �������� ��� ������� �� ��������� �������� �������� ��������. ���������� ����� �������� �������.
		///// </summary>
		//event CurrentChangingEventHandler CurrentChanging;

		/// <summary>
		/// ���������� ��������, ����������� ������ �� ���������� ������� � ���������.
		/// </summary>
		/// <param name="item">����������� �������</param>
		/// <returns>true ���� ������� ������������ � �����������; ����� false.</returns>
		bool Contains(object item);

		///// <summary>
		///// ������ ���������� ����, ������� ����� ������������ ��� ������� ��������� � ����������� � �������� ����������.
		///// </summary>
		///// <returns>������ System.IDisposable, ������� ����� ������������ ��� ������������.</returns>
		//IDisposable DeferRefresh();

		/// <summary>
		/// ������ ��������� ������� �������
		/// </summary>
		/// <param name="item">�������, ������� ����� ������� �������</param>
		/// <returns>true ���� � ���������� ICollectionView.CurrentItem ��������� � �����������; ����� false.</returns>
		bool MoveCurrentTo(object item);

		///// <summary>
		///// ������ ������ ������� �������.
		///// </summary>
		///// <returns>true ���� � ���������� ICollectionView.CurrentItem ��������� � �����������; ����� false.</returns>
		//bool MoveCurrentToFirst();

		///// <summary>
		///// ������ ��������� ������� �������.
		///// </summary>
		///// <returns>true ���� � ���������� ICollectionView.CurrentItem ��������� � �����������; ����� false.</returns>
		//bool MoveCurrentToLast();

		/// <summary>
		/// ������ ������� �������, ��������� �� ICollectionView.CurrentItem.
		/// </summary>
		/// <returns>true ���� � ���������� ICollectionView.CurrentItem ��������� � �����������; ����� false.</returns>
		bool MoveCurrentToNext();

		/// <summary>
		/// ������ ������� ������� ��� ��������� �������.
		/// </summary>
		/// <param name="position">������ ��������, ������� ����� ������� �������</param>
		/// <returns>true ���� � ���������� ICollectionView.CurrentItem ��������� � �����������; ����� false.</returns>
		bool MoveCurrentToPosition(int position);

		/// <summary>
		/// ������ ������� �������, �������������� ICollectionView.CurrentItem.
		/// </summary>
		/// <returns>true ���� � ���������� ICollectionView.CurrentItem ��������� � �����������; ����� false.</returns>
		bool MoveCurrentToPrevious();

		/// <summary>
		/// ����������� �����������
		/// </summary>
		void Refresh();
	}
}