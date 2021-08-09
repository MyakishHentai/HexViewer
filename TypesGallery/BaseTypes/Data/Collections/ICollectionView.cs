using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;

namespace Cryptosoft.TypesGallery.BaseTypes.Data
{
	/// <summary>
	/// Интерфейс поддерживающий функцилональность текущего элемента, сортировки, фильтрации и группировки
	/// </summary>
	public interface ICollectionView : IEnumerable, INotifyCollectionChanged
	{
		/// <summary>
		/// Значение, указывающее что данное отображение поддерживает фильтрацию с помощью свойства ICollectionView.Filter
		/// </summary>
		/// <returns>true если отображение поддерживает фильтрацию; иначе, false.</returns>
		bool CanFilter { get; }

		/// <summary>
		/// Значение, указывающее что данное отображение поддерживает группировку с помощью свойства ICollectionView.GroupDescriptions
		/// </summary>
		/// <returns>true если отображение поддерживает группировку; иначе, false.</returns>
		bool CanGroup { get; }

		/// <summary>
		/// Значение, указывающее что данное отображение поддерживает сортировку с помощью свойства ICollectionView.SortDescriptions
		/// </summary>
		/// <returns>true если отображение поддерживает сортировку; иначе, false.</returns>
		bool CanSort { get; }

		/// <summary>
		/// Возвращает или устанавливает культуру для операций, которые могут зависить от культуры, например сортировка.
		/// </summary>
		CultureInfo Culture { get; set; }

		/// <summary>
		/// Возвращает текущий элемент в отображении
		/// </summary>
		/// <returns>Текущий элемент или null, если нет текущего элемента.</returns>
		object CurrentItem { get; }

		/// <summary>
		/// Возвращает порядковый номер ICollectionView.CurrentItem в отображении.
		/// </summary>
		/// <returns>Позиция ICollectionView.CurrentItem в отображении.</returns>
		int CurrentPosition { get; }

		/// <summary>
		/// Получает или устанавливает обратный вызов, используемый для определения следует ли включать элемент в отображение.
		/// </summary>
		/// <returns>Метод, используемый для определения следует ли включать элемент в отображение.</returns>
		Predicate<object> Filter { get; set; }

		/// <summary>
		/// Возвращает коллекцию объектов GroupDescription, которые описывают как группируются элементы в отображении
		/// </summary>
		ObservableCollection<GroupDescription> GroupDescriptions { get; }

		/// <summary>
		/// Возвращает группы
		/// </summary>
		/// <returns>Коллекция групп, доступная только для чтения или null, если групп нет.</returns>
		ReadOnlyObservableCollection<object> Groups { get; }

		/// <summary>
		/// Возвращает значение, указывающее что ICollectionView.CurrentItem за пределами коллекции
		/// </summary>
		/// <returns>true если ICollectionView.CurrentItem за пределами коллекции; иначе false</returns>
		bool IsCurrentAfterLast { get; }

		/// <summary>
		/// Возвращает значение, указывающее что ICollectionView.CurrentItem за пределами коллекции
		/// </summary>
		/// <returns>true если ICollectionView.CurrentItem за пределами коллекции; иначе false</returns>
		bool IsCurrentBeforeFirst { get; }

		/// <summary>
		/// Возвращает значение, указывающее является ли отображение пустым
		/// </summary>
		/// <returns>true если отображение пустое; иначе false.</returns>
		bool IsEmpty { get; }

		/// <summary>
		/// Вазвращает коллекцию объектов SortDescription, определяющих как элементы сортируются в отображении.
		/// </summary>
		SortDescriptionCollection SortDescriptions { get; }

		/// <summary>
		/// Возвращает внутреннюю коллекцию
		/// </summary>
		IEnumerable SourceCollection { get; }

		/// <summary>
		/// При реализации интерфейса вызовите это событие после изменения значения текущего элемента.
		/// </summary>
		event EventHandler CurrentChanged;

		///// <summary>
		///// При реализации интерфейса вызовите это событие до изменения значения текущего элемента. Обработчик может отменить событие.
		///// </summary>
		//event CurrentChangingEventHandler CurrentChanging;

		/// <summary>
		/// Возвращает значение, указывающее входит ли переданный элемент в коллекцию.
		/// </summary>
		/// <param name="item">Проверяемый элемент</param>
		/// <returns>true если элемент присутствует в отображении; иначе false.</returns>
		bool Contains(object item);

		///// <summary>
		///// Вводит отложенный цикл, который можно использовать для слияния изменений в отображение и задержки обновления.
		///// </summary>
		///// <returns>Объект System.IDisposable, который можно использовать для освобождения.</returns>
		//IDisposable DeferRefresh();

		/// <summary>
		/// Делает указанный элемент текущим
		/// </summary>
		/// <param name="item">Элемент, который нужно сделать текущим</param>
		/// <returns>true если в результате ICollectionView.CurrentItem находится в отображении; иначе false.</returns>
		bool MoveCurrentTo(object item);

		///// <summary>
		///// Делает первый элемент текущим.
		///// </summary>
		///// <returns>true если в результате ICollectionView.CurrentItem находится в отображении; иначе false.</returns>
		//bool MoveCurrentToFirst();

		///// <summary>
		///// Делает последний элемент текущим.
		///// </summary>
		///// <returns>true если в результате ICollectionView.CurrentItem находится в отображении; иначе false.</returns>
		//bool MoveCurrentToLast();

		/// <summary>
		/// Делает текущим элемент, следующий за ICollectionView.CurrentItem.
		/// </summary>
		/// <returns>true если в результате ICollectionView.CurrentItem находится в отображении; иначе false.</returns>
		bool MoveCurrentToNext();

		/// <summary>
		/// Делает текущим элемент под указанным номером.
		/// </summary>
		/// <param name="position">Индекс элемента, который нужно сделать текущим</param>
		/// <returns>true если в результате ICollectionView.CurrentItem находится в отображении; иначе false.</returns>
		bool MoveCurrentToPosition(int position);

		/// <summary>
		/// Делает текущим элемент, предшествующий ICollectionView.CurrentItem.
		/// </summary>
		/// <returns>true если в результате ICollectionView.CurrentItem находится в отображении; иначе false.</returns>
		bool MoveCurrentToPrevious();

		/// <summary>
		/// Пересоздает отображение
		/// </summary>
		void Refresh();
	}
}