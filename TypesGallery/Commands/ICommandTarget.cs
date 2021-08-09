using Cryptosoft.TypesGallery.Events;

namespace Cryptosoft.TypesGallery.Commands
{
	public interface ICommandTarget : IRoutedEventsNode
	{
		CommandTargetSite CommandSite { get; }
	}

	public interface ICommandSource
	{
		CommandDescriptor Command { get; }

		object CommandArgument { get; set; }

		ICommandTarget CommandTarget { get; set; }
	}
}
