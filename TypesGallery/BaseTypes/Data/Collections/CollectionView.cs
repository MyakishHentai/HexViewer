using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Cryptosoft.TypesGallery.Events;

namespace Cryptosoft.TypesGallery.BaseTypes.Data
{
	public class CollectionView : ICollectionView, INotifyPropertyChanged
	{
		private CultureInfo m_Culture;

		private ArrayList m_InternalList;

		private readonly /*ICollection*/IEnumerable m_SourceCollection;

		private Predicate<object> m_Filter;
		private readonly SortDescriptionCollection m_SortDescriptions = new SortDescriptionCollection();
		private bool m_FilterChanged = true;

		public event NotifyCollectionChangedEventHandler CollectionChanged;
		public event PropertyChangedEventHandler PropertyChanged;

		//private readonly NotifyCollectionChangedEventHandler m_Handler;

		bool UseInternalList
		{
			get { return Filter != null || SortDescriptions.Count > 0 || GroupDescriptions != null; }
		}

		private void UpdateInternalList(bool reset = false)
		{
			if (UseInternalList)
			{
				// фильтрация

				if (m_FilterChanged || reset)
				{
					int InternalCount = 0;

					if (m_GetCount != null)
					{
						InternalCount = (int)m_GetCount.Invoke(m_SourceCollection, null);

						InternalCount = InternalCount > 100000 && Filter != null ? 100000 : InternalCount;
					}
					else
					{
						if (Filter == null)
						{
							InternalCount = m_SourceCollection.Cast<object>().Count();
						}
						else
						{
							foreach (var unused in m_SourceCollection)
							{
								InternalCount++;

								if (InternalCount > 100000)
									break;
							}
						}
					}

					m_InternalList = new ArrayList(InternalCount);

					if (Filter != null)
					{
						foreach (var OneItem in m_SourceCollection)
						{
							if (Filter(OneItem))
								m_InternalList.Add(OneItem);
						}
					}
					else
					{
						foreach (var OneItem in m_SourceCollection)
						{
							m_InternalList.Add(OneItem);
						}
					}

					m_FilterChanged = false;
				}

				// сортировка

				if (SortDescriptions.Count > 0)
				{
					m_InternalList.Sort(Comparer);
				}
			}

			// группирование

			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		public IComparer Comparer
		{
			get { return new SortDescriptionCollectionComparer(m_SortDescriptions); }
		}

		public IEnumerable SourceCollection
		{
			get { return m_SourceCollection; }
		}

		public event EventHandler CurrentChanged;

		public bool CanFilter
		{
			get { return true; }
		}

		public bool CanGroup
		{
			get { return false; }
		}

		public bool CanSort
		{
			get { return true; }
		}

		public CultureInfo Culture
		{
			get { return m_Culture ?? CultureInfo.CurrentCulture; }
			set { m_Culture = value; }
		}

		public object CurrentItem { get; private set; }
		public int CurrentPosition { get; private set; }

		public Predicate<object> Filter
		{
			get { return m_Filter; }

			set
			{
				if (m_Filter != value)
				{
					m_Filter = value;
					m_FilterChanged = true;
					UpdateInternalList();
				}
			}
		}

		public ObservableCollection<GroupDescription> GroupDescriptions
		{
			get { return null; }
		}

		public ReadOnlyObservableCollection<object> Groups
		{
			get { return null; }
		}

		public bool IsCurrentAfterLast
		{
			get { return CurrentPosition < 0; }
		}

		public bool IsCurrentBeforeFirst { get { return CurrentPosition >= Count; } }

		public bool IsEmpty
		{
			get
			{
				if (UseInternalList)
					return m_InternalList.Count == 0;

				if (m_GetCount != null)
					return 0 == (int)m_GetCount.Invoke(m_SourceCollection, null);

				return !m_SourceCollection.Cast<object>().Any();
			}
		}

		public SortDescriptionCollection SortDescriptions
		{
			get { return m_SortDescriptions; }
		}

		/// <summary>
		/// Метод, используемый для получения элемента под указанным индексом
		/// </summary>
		readonly MethodInfo m_GetItem;
		readonly MethodInfo m_GetCount;

		public CollectionView(IEnumerable source)
		{
			if (source == null)
				throw new ArgumentNullException("source");

			//m_Handler = OnCollectionChanged;

			//Получение элемента коллекции по индексу или ключу
			m_GetItem = source.GetType().GetMethod("get_Item", new[] { typeof(int) });

			m_GetCount = source.GetType().GetMethod("get_Count");

			m_SourceCollection = source;

			UpdateInternalList();

			var NotifyCollection = m_SourceCollection as INotifyCollectionChanged;

			if (NotifyCollection != null)
				CollectionChangedEventManager.AddListener(NotifyCollection, /*m_Handler*/OnCollectionChanged);
		}

		private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (UseInternalList)
			{
				switch (e.Action)
				{
					case NotifyCollectionChangedAction.Add:
						if (Filter != null && !Filter(e.NewItems[0]))
							break;

						object ItemToAdd = e.NewItems[0];
						m_InternalList.Add(ItemToAdd);
						OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, ItemToAdd));
						break;

					case NotifyCollectionChangedAction.Remove:
						object ItemToRemove = e.OldItems[0];
						m_InternalList.Remove(ItemToRemove);
						OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, ItemToRemove));
						break;

					case NotifyCollectionChangedAction.Replace:
						throw new NotImplementedException();

					case NotifyCollectionChangedAction.Move:
						throw new NotImplementedException();

					case NotifyCollectionChangedAction.Reset:
						UpdateInternalList(true);
						break;

					default:
						throw new ArgumentOutOfRangeException();
				}
			}
			else
			{
				OnCollectionChanged(e);
			}
		}

		public bool Contains(object item)
		{
			if (UseInternalList)
				return m_InternalList.IndexOf(item) >= 0;

			throw new NotImplementedException();
			//return m_SourceCollection.IndexOf(item) >= 0;
		}

		public bool MoveCurrentTo(object item)
		{
			int Index = IndexOf(item);

			if (Index < 0)
			{
				CurrentItem = null;
				CurrentPosition = Index;

				if (CurrentChanged != null)
					CurrentChanged(this, EventArgs.Empty);

				return false;
			}

			if (CurrentItem != item)
			{
				CurrentItem = item;
				CurrentPosition = Index;

				if (CurrentChanged != null)
					CurrentChanged(this, EventArgs.Empty);
			}

			return true;
		}

		public int IndexOf(object item)
		{
			if (UseInternalList)
			{
				return m_InternalList.IndexOf(item);
			}

			int Index = 0;

			foreach (var Item in m_SourceCollection)
			{
				if (Item == item)
					return Index;

				Index++;
			}

			return -1;
		}

		public bool MoveCurrentToNext()
		{
			if (CurrentPosition >= Count - 1)
				return false;

			CurrentPosition++;
			CurrentItem = this[CurrentPosition];

			if (CurrentChanged != null)
				CurrentChanged(this, EventArgs.Empty);

			return true;
		}

		public bool MoveCurrentToPosition(int position)
		{
			if (CurrentPosition >= Count)
				return false;

			if (CurrentPosition < 0)
				return false;

			CurrentPosition = position;
			CurrentItem = this[CurrentPosition];

			if (CurrentChanged != null)
				CurrentChanged(this, EventArgs.Empty);

			return true;
		}

		public bool MoveCurrentToPrevious()
		{
			if (CurrentPosition <= 0)
				return false;

			CurrentPosition--;
			CurrentItem = this[CurrentPosition];

			if (CurrentChanged != null)
				CurrentChanged(this, EventArgs.Empty);

			return true;
		}

		public void Refresh()
		{
			UpdateInternalList(true);
		}

		public IEnumerator GetEnumerator()
		{
			if (UseInternalList)
				return m_InternalList.GetEnumerator();

			return m_SourceCollection.GetEnumerator();
		}

		protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			var Handler = CollectionChanged;
			if (Handler != null) Handler(this, e);
		}

		protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
		{
			var Handler = PropertyChanged;
			if (Handler != null) Handler(this, e);
		}

		/// <summary>
		/// Сообщает количество элементов в обернутой коллекции
		/// </summary>
		public int Count
		{
			get
			{
				if (UseInternalList)
				{
					return m_InternalList.Count;
				}

				if (m_SourceCollection != null)
				{
					if (m_GetCount != null)
						return (int)m_GetCount.Invoke(m_SourceCollection, null);
				}

				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Сообщает элементом под индексом в обернутой коллекции
		/// </summary>
		/// <param name="index">Индекс элемента</param>
		/// <returns>Элемент под указанным индексом</returns>
		public object this[int index]
		{
			get
			{
				if (UseInternalList)
				{
					return m_InternalList[index];
				}

				//Если : полученный объект поддерживает индексацию
				if (m_GetItem != null)
				{
					return m_GetItem.Invoke(m_SourceCollection, new object[] { index });
				}

				throw new NotImplementedException();
			}
		}

		protected class SortDescriptionCollectionComparer : IComparer
		{
			private readonly SortDescriptionCollection m_SortDescription;

			public SortDescriptionCollectionComparer(SortDescriptionCollection sort)
			{
				m_SortDescription = sort;
			}

			public int Compare(object x, object y)
			{
				if (x == null)
				{
					if (y == null)
						return 0;

					return -1;
				}

				if (y == null)
					return 1;

				foreach (var Description in m_SortDescription)
				{
					var DescriptionType = Description.GetType();

					if (DescriptionType == typeof(PropertySortDescription))
					{
						var PropertyDescription = Description as PropertySortDescription;
						if (PropertyDescription != null)
						{
                            int Result = 0;

                            try
                            {
                                var ValueX = GetPropertyValue(x, PropertyDescription.PropertyName);
                                var ValueY = GetPropertyValue(y, PropertyDescription.PropertyName);
                                Result = System.Collections.Comparer.Default.Compare(ValueX, ValueY);
                            }
                            catch (NullReferenceException e)
                            {
                                Log.Verbose(e.Data);
                            }

							if (Result != 0)
								return PropertyDescription.Direction == ListSortDirection.Ascending ? Result : -Result;
						}
					}
					else if (DescriptionType == typeof(CustomSortDescription))
					{
						var TypeDescription = Description as CustomSortDescription;
						if (TypeDescription != null)
						{
							int Result = TypeDescription.Comparer.Compare(x, y);

							if (Result != 0)
								return TypeDescription.Direction == ListSortDirection.Ascending ? Result : -Result;
						}
					}
				}

				return 0;
			}

            private object GetPropertyValue(object obj, string name)
            {
                Type ObjType = obj.GetType();
                PropertyInfo Property;
                                
                if (name.Contains("."))
                {
                    int Index = name.IndexOf(".");
                    Property = ObjType.GetProperty(name.Substring(0, Index),
                        BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.Public);
                    Index++;
                    return GetPropertyValue(Property.GetValue(obj), name.Substring(Index, name.Length - Index));
                }

                Property = ObjType.GetProperty(name,
                    BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.Public);
                return Property.GetValue(obj);
            }
		}
	}

	public static class CollectionViewHelper
	{
		public static void InvalidateItems(this IEnumerable collection)
		{
			CollectionView View = collection as CollectionView;
			if (View != null)
			{
				View.Refresh();
			}
		}
	}
}