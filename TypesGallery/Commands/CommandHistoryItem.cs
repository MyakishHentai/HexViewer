namespace Cryptosoft.TypesGallery.Commands
{
	public class CommandHistoryItem
	{
		public ICommand Command { get; private set; }

		public ICommandTarget Target { get; private set; }

		public CommandHistoryItem(ICommand command, ICommandTarget target, object parameter)
		{
			Command = command;
			Target = target;
		}

		public void Undo()
		{

		}

		public void Redo()
		{

		}
	}
}
