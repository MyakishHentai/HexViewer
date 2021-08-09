using System;

namespace Cryptosoft.TypesGallery.Events
{
	public class RoutedEventArgs : EventArgs
	{
		public bool Handled { get; set; }

		public RoutedEvent RoutedEvent { get; set; }

		public object Source { get; set; }

		public object OriginalSource { get; set; }

		public RoutedEventArgs()
		{ }

		public RoutedEventArgs(RoutedEvent routedEvent)
		{
			RoutedEvent = routedEvent;
		}
	}
}
