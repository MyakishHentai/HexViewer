using System.Threading;

namespace Cryptosoft.TypesGallery.Events
{
	public interface IRoutedEventsNode
	{
		IRoutedEventsNode RoutedParent { get; }

		RoutedEventSite RoutedSite { get; }
	}
}
