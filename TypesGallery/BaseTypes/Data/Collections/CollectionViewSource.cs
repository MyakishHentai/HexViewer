using System;
using System.Collections;
using System.Runtime.CompilerServices;

namespace Cryptosoft.TypesGallery.BaseTypes.Data
{
	public class CollectionViewSource
	{
		private static readonly ConditionalWeakTable<object, ICollectionView> s_DefaultViews = new ConditionalWeakTable<object, ICollectionView>();
		private IEnumerable m_Source;
		private Predicate<object> m_Filter;

		public Predicate<object> Filter
		{
			get { return m_Filter; }
			set
			{
				m_Filter = value;

				if (View != null)
					View.Filter = value;
			}
		}

		public IEnumerable Source
		{
			get { return m_Source; }
			set
			{
				m_Source = value; 
				UpdateView();
			}
		}

		public ICollectionView View { get; private set; }

		public static ICollectionView GetDefaultView(IEnumerable source)
		{
			if (source == null)
				return null;

			ICollectionView Result = source as ICollectionView;

			if (Result != null)
				return Result;

			if (s_DefaultViews.TryGetValue(source, out Result))
				return Result;

			IList List = source as IList;

			Result = List != null ? new ListCollectionView(List) : new CollectionView(source);


			s_DefaultViews.Add(source, Result);

			return Result;
		}

		private void UpdateView()
		{
			if (m_Source == null)
			{
				View = null;
				return;
			}

			ICollectionView Result = m_Source as ICollectionView;

			if (Result != null)
			{
				View = Result;
				return;
			}

			IList List = m_Source as IList;
			ICollection Collection = m_Source as ICollection;

			if (List != null)
			{
				View = new ListCollectionView(List);
			}
			else if (Collection != null)
			{
				View = new CollectionView(Collection);
			}
			else
			{
				throw new NotImplementedException();
			}

			View.Filter = Filter;
		}
	}
}