using System.Collections.Specialized;

namespace Cryptosoft.TypesGallery.Events
{
	public class CollectionChangedEventManager : WeakEventTable<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>
	{
		private static readonly CollectionChangedEventManager s_Current = new CollectionChangedEventManager();

		public static void AddListener(object source, NotifyCollectionChangedEventHandler handler)
		{
			s_Current.ProtectedAddListener(source, handler);
		}

		public static void RemoveListener(object source, NotifyCollectionChangedEventHandler handler)
		{
			s_Current.ProtectedRemoveListener(source, handler);
		}

		protected override void StartListening(object source)
		{
			((INotifyCollectionChanged) source).CollectionChanged += CollectionChangedEventManager_CollectionChanged;
		}

		private void CollectionChangedEventManager_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			DeliverEvent(sender, e);
		}

		protected override void StopListening(object source)
		{
			((INotifyCollectionChanged) source).CollectionChanged -= CollectionChangedEventManager_CollectionChanged;
		}

		protected override void InvokeEvent(NotifyCollectionChangedEventHandler handler, object sender, NotifyCollectionChangedEventArgs args)
		{
			handler.Invoke(sender, args);
		}
	}
}