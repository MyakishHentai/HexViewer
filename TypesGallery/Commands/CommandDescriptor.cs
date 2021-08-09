using System;
using System.Threading.Tasks;
using Cryptosoft.TypesGallery.Events;

namespace Cryptosoft.TypesGallery.Commands
{
	public static class CommandHelper
	{
		public class CommandConstructor
		{
			public CommandDescriptor Descriptor { get; private set; }

			internal ICommandTarget Target { get; private set; }

			internal object Argument { get; set; }

			internal CommandConstructor(CommandDescriptor commandDescriptor, ICommandTarget target)
			{
				Target = target;
				Descriptor = commandDescriptor;
			}

			public bool CanExecute()
			{
				return Descriptor.CanExecute(Argument, Target);
			}

			public ICommand Generate()
			{
				return Descriptor.GenerateCommand(Argument, Target);
			}

			public ICommand Execute()
			{
				ICommand Command = Generate();

				Command.Execute();
				
				return Command;
			}


			public async Task<ICommand> ExecuteAsync()
			{
				ICommand Command = Generate();

				var AsyncCommand = Command as IAsyncCommand;
				if (AsyncCommand != null && AsyncCommand.IsAsync)
				{
					await AsyncCommand.ExecuteAsync();
				}
				else
				{
					Command.Execute();
				}

				return Command;
			}
		}

		public static CommandConstructor For(this CommandDescriptor commandDescriptor, ICommandTarget target)
		{
			return new CommandConstructor(commandDescriptor, target);
		}

		public static CommandConstructor WithArgument(this CommandConstructor command, object argument)
		{
			command.Argument = argument;

			return command;
		}

		public static bool Is(this ICommand command, CommandDescriptor commandDescriptor)
		{
			IExtendCommand Command = command as IExtendCommand;

			return Command != null && Command.Descriptor == commandDescriptor;
		}
	}

	public class CommandDescriptor
	{
		public string Name { get; private set; }

		public Type OwnerType { get; private set; }

		public CommandDescriptor(string name, Type ownerType)
		{
			Name = name;
			OwnerType = ownerType;
		}

		public bool CanExecute(object parameter, ICommandTarget target)
		{
			if (target == null)
				return false;

			var NewCommand = GenerateCommandInternal(parameter, target, target);

			var Args = new CanExecuteEventArgs(NewCommand, parameter)
			{
				RoutedEvent = CommandManager.PreviewCanExecuteEvent,
				Source = NewCommand.Target,
				OriginalSource = NewCommand.OriginalTarget
			};

			target.RoutedSite.RaiseEvent(Args);

			if (Args.Handled)
				return false;

			Args.CanExecute = NewCommand.CanExecute();

			Args.RoutedEvent = CommandManager.CanExecuteEvent;
			target.RoutedSite.RaiseEvent(Args);

			return Args.CanExecute;
		}

		public ICommand GenerateCommand(object parameter, ICommandTarget target)
		{
			return GenerateCommandInternal(parameter, target, target);
		}

		private CommandWrapper GenerateCommandInternal(object parameter, IRoutedEventsNode target, ICommandTarget originalTarget)
		{
			var Bind = CommandManager.GetBinding(this, target);

			if (Bind == null)
			{
				return GenerateCommandInternal(parameter, target.RoutedParent, originalTarget);
			}

			return new CommandWrapper(Bind, parameter, target as ICommandTarget, originalTarget);
		}

		internal sealed class CommandWrapper : IExtendCommand, IAsyncCommand
		{
			private readonly ICommand m_Command;
			private readonly IAsyncCommand m_AsyncCommand;
			private readonly object m_Parameter;
			private readonly ICommandTarget m_Target;
			private readonly ICommandTarget m_OriginalTarget;
			private readonly CustomCommandBinding m_Binding;
			private bool m_Executed;

			public CommandWrapper(CustomCommandBinding binding, object parameter, ICommandTarget target, ICommandTarget originalTarget)
			{
				m_Command = binding.GenerateCommand(target, parameter);
				m_AsyncCommand = m_Command as IAsyncCommand;
				m_Parameter = parameter;
				m_Target = target;
				m_OriginalTarget = originalTarget;
				m_Binding = binding;
			}

			public CommandWrapper(CommandWrapper command, object parameter)
			{
				m_Command = command.m_Binding.GenerateCommand(command.m_Target, parameter);
				m_AsyncCommand = m_Command as IAsyncCommand;
				m_Parameter = parameter;
				m_Target = command.m_Target;
				m_OriginalTarget = command.m_OriginalTarget;
				m_Binding = command.m_Binding;
			}

			public ICommandTarget OriginalTarget { get { return m_OriginalTarget; } }
			public ICommandTarget Target { get { return m_Target; } }
			public CommandDescriptor Descriptor { get { return m_Binding.CommandDescriptor; } }

			bool IAsyncCommand.IsAsync { get { return m_AsyncCommand != null && m_AsyncCommand.IsAsync; } }

			public bool CanCancel()
			{
				if (!m_Executed)
					return false;

				var Args = new CanCancelEventArgs(this, m_Parameter)
				{
					RoutedEvent = CommandManager.PreviewCanCancelEvent,
					Source = m_Target,
					OriginalSource = m_OriginalTarget
				};

				m_OriginalTarget.RoutedSite.RaiseEvent(Args);

				if (Args.Handled)
					return false;

				Args.CanCancel = m_Command.CanCancel();

				Args.RoutedEvent = CommandManager.CanCancelEvent;
				m_OriginalTarget.RoutedSite.RaiseEvent(Args);

				return Args.CanCancel;
			}

			public void Cancel()
			{
				var Args = new CancelledEventArgs(this, m_Parameter)
				{
					RoutedEvent = CommandManager.PreviewCancelledEvent,
					Source = m_Target,
					OriginalSource = m_OriginalTarget
				};

				m_OriginalTarget.RoutedSite.RaiseEvent(Args);

				if (Args.Handled)
					return;

				m_Command.Cancel();

				m_Executed = false;

				Args.RoutedEvent = CommandManager.CancelledEvent;
				m_OriginalTarget.RoutedSite.RaiseEvent(Args);
			}

			public bool CanExecute()
			{
				if (m_Executed)
					return false;

				var Args = new CanExecuteEventArgs(this, m_Parameter)
				{
					RoutedEvent = CommandManager.PreviewCanExecuteEvent,
					Source = m_Target,
					OriginalSource = m_OriginalTarget
				};

				m_OriginalTarget.RoutedSite.RaiseEvent(Args);

				if (Args.Handled)
					return false;

				Args.CanExecute = m_Command.CanExecute();

				Args.RoutedEvent = CommandManager.CanExecuteEvent;
				m_OriginalTarget.RoutedSite.RaiseEvent(Args);

				return Args.CanExecute;
			}

			public void Execute()
			{
				var Args = new ExecutedEventArgs(this, m_Parameter)
				{
					RoutedEvent = CommandManager.PreviewExecutedEvent,
					Source = m_Target,
					OriginalSource = m_OriginalTarget
				};

				m_OriginalTarget.RoutedSite.RaiseEvent(Args);

				if (Args.Handled)
					return;

				try
				{
					m_Command.Execute();
				}
				catch (Exception Ex)
				{
					Log.Verbose("Команда " + this, Ex);
					Args.Exception = Ex;
				}

				m_Executed = true;

				Args.RoutedEvent = CommandManager.ExecutedEvent;
				m_OriginalTarget.RoutedSite.RaiseEvent(Args);

				if (Args.RethrowException && Args.Exception != null)
					throw Args.Exception;
			}

			public override string ToString()
			{
				return m_Binding.CommandDescriptor.Name;
			}

			public async Task ExecuteAsync()
			{
				var Args = new ExecutedEventArgs(this, m_Parameter)
				{
					RoutedEvent = CommandManager.PreviewExecutedEvent,
					Source = m_Target,
					OriginalSource = m_OriginalTarget
				};

				m_OriginalTarget.RoutedSite.RaiseEvent(Args);

				if (Args.Handled)
					return;

				try
				{
					await m_AsyncCommand.ExecuteAsync();
				}
				catch (Exception Ex)
				{
					Log.Verbose("Команда " + this, Ex);
					Args.Exception = Ex;
				}

				m_Executed = true;

				Args.RoutedEvent = CommandManager.ExecutedEvent;
				m_OriginalTarget.RoutedSite.RaiseEvent(Args);

				if (Args.RethrowException && Args.Exception != null)
					throw Args.Exception;
			}

			public async Task CancelAsync()
			{
				var Args = new CancelledEventArgs(this, m_Parameter)
				{
					RoutedEvent = CommandManager.PreviewCancelledEvent,
					Source = m_Target,
					OriginalSource = m_OriginalTarget
				};

				m_OriginalTarget.RoutedSite.RaiseEvent(Args);

				if (Args.Handled)
					return;

				await m_AsyncCommand.CancelAsync();

				m_Executed = false;

				Args.RoutedEvent = CommandManager.CancelledEvent;
				m_OriginalTarget.RoutedSite.RaiseEvent(Args);
			}
		}
	}
}