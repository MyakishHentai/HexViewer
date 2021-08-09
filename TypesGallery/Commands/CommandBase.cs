using System.Threading.Tasks;

namespace Cryptosoft.TypesGallery.Commands
{
	public abstract class CommandBase : ICommand
	{
		public virtual bool CanCancel()
		{
			return false;
		}

		public virtual void Cancel()
		{
		}

		public virtual bool CanExecute()
		{
			return true;
		}

		public abstract void Execute();
	}

	public abstract class AsyncCommandBase : CommandBase, IAsyncCommand
	{
		static Task s_CompletedTask = Task.Run(() => { });

		public bool IsAsync { get { return true; } }

		public sealed override void Execute()
		{
			ExecuteAsync().Wait();
		}

		public sealed override void Cancel()
		{
			CancelAsync().Wait();
		}

		public abstract Task ExecuteAsync();

		public virtual Task CancelAsync()
		{
			return s_CompletedTask;
		}
	}
}
