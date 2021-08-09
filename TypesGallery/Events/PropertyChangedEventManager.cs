using System.ComponentModel;

namespace Cryptosoft.TypesGallery.Events
{
	public class PropertyChangedEventManager : WeakEventTable<PropertyChangedEventHandler, PropertyChangedEventArgs>
	{
		private static readonly PropertyChangedEventManager s_Current = new PropertyChangedEventManager();

		public static void AddListener(object source, PropertyChangedEventHandler handler)
		{
			s_Current.ProtectedAddListener(source, handler);
		}

		public static void RemoveListener(object source, PropertyChangedEventHandler handler)
		{
			s_Current.ProtectedRemoveListener(source, handler);
		}

		protected override void StartListening(object source)
		{
			((INotifyPropertyChanged)source).PropertyChanged += CollectionChangedEventManager_CollectionChanged;
		}

		private void CollectionChangedEventManager_CollectionChanged(object sender, PropertyChangedEventArgs e)
		{
			DeliverEvent(sender, e);
		}

		protected override void StopListening(object source)
		{
			((INotifyPropertyChanged)source).PropertyChanged -= CollectionChangedEventManager_CollectionChanged;
		}

		protected override void InvokeEvent(PropertyChangedEventHandler handler, object sender, PropertyChangedEventArgs args)
		{
			handler.Invoke(sender, args);
		}
	}
}