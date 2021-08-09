using System;

namespace Cryptosoft.TypesGallery.Events
{
	public sealed class RoutedEvent
	{
		public RoutingStrategy RoutingStrategy { get; private set; }

		public string Name { get; private set; }

		private RoutedEvent()
		{
		}

		public static RoutedEvent Register(string name, RoutingStrategy routingStrategy, Type handlerType, Type ownerType)
		{
			RoutedEvent Event = new RoutedEvent();
			Event.Name = name;
			Event.RoutingStrategy = routingStrategy;

			return Event;
		}
	}

	public enum RoutingStrategy
	{
		Tunnel = 0,
		Bubble = 1,
		Direct = 2
	}
}
