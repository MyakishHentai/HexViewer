using System.Threading.Tasks;

namespace Cryptosoft.TypesGallery.Commands
{
	public interface ICommand
	{
		//event EventHandler CanExecuteChanged;
		bool CanExecute();

		void Execute();

		bool CanCancel();

		void Cancel();
	}

	public interface IAsyncCommand : ICommand
	{
		bool IsAsync { get; }

		Task ExecuteAsync();

		Task CancelAsync();
	}

	public interface IExtendCommand : ICommand
	{
		CommandDescriptor Descriptor { get; }
	}
}