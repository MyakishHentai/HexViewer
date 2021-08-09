using System;
using Cryptosoft.TypesGallery.Events;

namespace Cryptosoft.TypesGallery.Commands
{
	class CustomCommandBinding
	{
		public CommandDescriptor CommandDescriptor { get; private set; }

		private readonly GenerateCommandHandler m_Generate;

		public CustomCommandBinding(CommandDescriptor command, GenerateCommandHandler generate)
		{
			CommandDescriptor = command;
			m_Generate = generate;
		}

		public ICommand GenerateCommand(object target, object parameter)
		{
			return m_Generate(target, parameter);
		}
	}

	public delegate ICommand GenerateCommandHandler(object target, object parameter);

	public sealed class ExecutedEventArgs : RoutedEventArgs
	{
		public ICommand Command { get; private set; }

		public object Parameter { get; private set; }

		public Exception Exception { get; internal set; }

		public bool RethrowException { get; set; }

		public ExecutedEventArgs(ICommand command, object parameter)
		{
			RethrowException = true;
			Command = command;
			Parameter = parameter;
		}
	}

	public sealed class CanExecuteEventArgs : RoutedEventArgs
	{
		public bool CanExecute { get; set; }
		public ICommand Command { get; private set; }
		public object Parameter { get; private set; }

		public CanExecuteEventArgs(ICommand command, object parameter)
		{
			CanExecute = true;
			Command = command;
			Parameter = parameter;
		}
	}

	public sealed class CancelledEventArgs : RoutedEventArgs
	{
		public ICommand Command { get; private set; }

		public object Parameter { get; private set; }

		public CancelledEventArgs(ICommand command, object parameter)
		{
			Command = command;
			Parameter = parameter;
		}
	}

	public sealed class CanCancelEventArgs : RoutedEventArgs
	{
		public bool CanCancel { get; set; }
		public ICommand Command { get; private set; }
		public object Parameter { get; private set; }

		public CanCancelEventArgs(ICommand command, object parameter)
		{
			CanCancel = true;
			Command = command;
			Parameter = parameter;
		}
	}

}
