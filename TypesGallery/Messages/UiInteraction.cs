using Cryptosoft.TypesGallery.Events;
using System.Threading.Tasks;

namespace Cryptosoft.TypesGallery
{
	public delegate void UiMessageEventHandler(object sender, UiMessageEventArgs e);

	public static class UiInteraction
	{
		public static RoutedEvent MessageEvent = RoutedEvent.Register("UiMessage", RoutingStrategy.Bubble, typeof(UiMessageEventHandler), typeof(UiInteraction));

		public static async Task<UiMessageEventArgs> SendUiMessage(this RoutedEventSite site, string caption, string message, ChoiceCollection choises = null, object data = null)
		{
			var Args = new UiMessageEventArgs
			{
				OriginalSource = site.Owner,
				Source = site.Owner,
				RoutedEvent = MessageEvent,
				Caption = caption,
				Message = message,
				ExtendedData = data,
				Choices = choises
			};

			site.RaiseEvent(Args);

			await Args.WaitForReturnAsync();

			return Args;
		}

	}
}