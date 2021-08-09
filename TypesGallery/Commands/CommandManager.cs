using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Cryptosoft.TypesGallery.Events;

namespace Cryptosoft.TypesGallery.Commands
{
	public delegate void ExecutedEventHandler(object sender, ExecutedEventArgs e);
	public delegate void CanExecuteEventHandler(object sender, CanExecuteEventArgs e);

	public delegate void CancelledEventHandler(object sender, CancelledEventArgs e);
	public delegate void CanCancelEventHandler(object sender, CanCancelEventArgs e);

	public static class CommandManager
	{
		public static RoutedEvent PreviewExecutedEvent = RoutedEvent.Register("PreviewExecuted", RoutingStrategy.Tunnel, typeof(ExecutedEventHandler), typeof(CommandManager));
		public static RoutedEvent ExecutedEvent = RoutedEvent.Register("Executed", RoutingStrategy.Bubble, typeof(ExecutedEventHandler), typeof(CommandManager));
		public static RoutedEvent PreviewCanExecuteEvent = RoutedEvent.Register("PreviewCanExecute", RoutingStrategy.Tunnel, typeof(CanExecuteEventHandler), typeof(CommandManager));
		public static RoutedEvent CanExecuteEvent = RoutedEvent.Register("CanExecute", RoutingStrategy.Bubble, typeof(CanExecuteEventHandler), typeof(CommandManager));

		public static RoutedEvent PreviewCancelledEvent = RoutedEvent.Register("PreviewCancelled", RoutingStrategy.Tunnel, typeof(CancelledEventHandler), typeof(CommandManager));
		public static RoutedEvent CancelledEvent = RoutedEvent.Register("Cancelled", RoutingStrategy.Bubble, typeof(CancelledEventHandler), typeof(CommandManager));
		public static RoutedEvent PreviewCanCancelEvent = RoutedEvent.Register("PreviewCanCancel", RoutingStrategy.Tunnel, typeof(CanCancelEventHandler), typeof(CommandManager));
		public static RoutedEvent CanCancelEvent = RoutedEvent.Register("CanCancel", RoutingStrategy.Bubble, typeof(CanCancelEventHandler), typeof(CommandManager));


		private static readonly ConditionalWeakTable<Type, Dictionary<CommandDescriptor, CustomCommandBinding>> s_ClassBindings = new ConditionalWeakTable<Type, Dictionary<CommandDescriptor, CustomCommandBinding>>();
		private static readonly ConditionalWeakTable<object, Dictionary<CommandDescriptor, CustomCommandBinding>> s_ObjectsBindings = new ConditionalWeakTable<object, Dictionary<CommandDescriptor, CustomCommandBinding>>();

		public static void RegisterClassCommand(Type type, CommandDescriptor command, GenerateCommandHandler generateCommand)
		{
			s_ClassBindings.GetOrCreateValue(type)[command] = new CustomCommandBinding(command, generateCommand);
		}

		public static void RegisterCommand(object obj, CommandDescriptor command, GenerateCommandHandler generateCommand)
		{
			s_ObjectsBindings.GetOrCreateValue(obj)[command] = new CustomCommandBinding(command, generateCommand);
		}

		internal static CustomCommandBinding GetBinding(CommandDescriptor command, object target)
		{
			Type TargetType = target.GetType();

			while (TargetType != null)
			{
				Dictionary<CommandDescriptor, CustomCommandBinding> Bindings;

				if (s_ClassBindings.TryGetValue(TargetType, out Bindings) && Bindings.ContainsKey(command))
					return Bindings[command];

				TargetType = TargetType.BaseType;
			}

			Dictionary<CommandDescriptor, CustomCommandBinding> ObjectBindings;

			if (s_ObjectsBindings.TryGetValue(target, out ObjectBindings) && ObjectBindings.ContainsKey(command))
				return ObjectBindings[command];

			return null;
		}
	}
}
