using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Cryptosoft.TypesGallery.BaseTypes.Data
{
	/// <summary>
	/// Представляет коллекцию объектов SortDescription
	/// </summary>
	public class SortDescriptionCollection : Collection</*SortDescription*/SortDescriptionBase>, INotifyCollectionChanged
	{
		/// <summary>
		/// Возвращает пустой немодифицируемый экземпляр SortDescriptionCollection.
		/// </summary>
		public static readonly SortDescriptionCollection Empty = new EmptySortDescriptionCollection();

		/// <summary>
		/// Инициализирует новый экземпляр класса SortDescriptionCollection
		/// </summary>
		public SortDescriptionCollection()
		{
		}

		/// <summary>Случается когда добавляется или удаляется элемент.</summary>
		protected event NotifyCollectionChangedEventHandler CollectionChanged;

		event NotifyCollectionChangedEventHandler INotifyCollectionChanged.CollectionChanged
		{
			add { CollectionChanged += value; }
			remove { CollectionChanged -= value; }
		}

		/// <summary>
		/// Удаляет все элементы из коллекции
		/// </summary>
		protected override void ClearItems()
		{
			base.ClearItems();
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		/// <summary>
		/// Вставляет элемент в позицию с указанным индексом
		/// </summary>
		/// <param name="index">Индекс, по которому всавляется элемент.</param>
		/// <param name="item">Добавляемый элемент.</param>
		protected override void InsertItem(int index, /*SortDescription*/SortDescriptionBase item)
		{
			base.InsertItem(index, item);
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
		}

		/// <summary>
		/// Удаляет элемент в указанной позиции
		/// </summary>
		/// <param name="index">Индекс удаляемого элемента.</param>
		protected override void RemoveItem(int index)
		{
			object RemovedItem = this[index];
			base.RemoveItem(index);
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, RemovedItem, index));
		}

		/// <summary>
		/// Заменяет элемент по указанному индексу
		/// </summary>
		/// <param name="index">Индекс заменемого элемента</param>
		/// <param name="item">Новое значение элемента</param>
		protected override void SetItem(int index, /*SortDescription*/SortDescriptionBase item)
		{
			object RemovedItem = this[index];
			base.SetItem(index, item);
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, RemovedItem, index));
		}

		protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			var Handler = CollectionChanged;
			if (Handler != null) Handler(this, e);
		}
	}

	class EmptySortDescriptionCollection : SortDescriptionCollection
	{
		protected override void ClearItems()
		{
		}

		protected override void InsertItem(int index, /*SortDescription*/SortDescriptionBase item)
		{
		}

		protected override void RemoveItem(int index)
		{
		}

		protected override void SetItem(int index, /*SortDescription*/SortDescriptionBase item)
		{
		}
	}
}