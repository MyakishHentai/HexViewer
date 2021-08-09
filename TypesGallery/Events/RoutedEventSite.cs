using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace Cryptosoft.TypesGallery.Events
{
	public static class SynchronizationHelper
	{
		private static readonly object s_SynchronizationObject = new object();

		private static SynchronizationContext s_SynchronizationContext;

		public static SynchronizationContext CurrentContext
		{
			get
			{
				lock (s_SynchronizationObject)
				{
					return s_SynchronizationContext ?? (s_SynchronizationContext = SynchronizationContext.Current ?? new SynchronizationContext());
				}
			}
		}

		public static void ClearCurrentSynchronizationContext()
		{
			lock (s_SynchronizationObject)
			{
				s_SynchronizationContext = null;
			}
		}

		public static void UpdateCurrentSynchronizationContext()
		{
			lock (s_SynchronizationObject)
			{
				s_SynchronizationContext = SynchronizationContext.Current ?? new SynchronizationContext();
			}
		}
	}

	public sealed class RoutedEventSite
	{
		private class FastInvokeDelegateCollection : IEnumerable<FastInvokeDelegate>
		{
			private readonly List<FastInvokeDelegate> m_List = new List<FastInvokeDelegate>();

			public IEnumerator<FastInvokeDelegate> GetEnumerator()
			{
				return m_List.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return ((IEnumerable)m_List).GetEnumerator();
			}

			public void Add(Delegate handler)
			{
				m_List.Add(new FastInvokeDelegate(handler));
			}

			public void Remove(Delegate handler)
			{
				int Index = m_List.FindLastIndex(del => del.Delegate == handler);
				if (Index >= 0)
					m_List.RemoveAt(Index);
			}
		}

		private class FastInvokeDelegate
		{
			public Delegate Delegate { get; private set; }

			private Action<object, RoutedEventArgs> m_CompiledDelegate;

			public FastInvokeDelegate(Delegate delegateItem)
			{
				Delegate = delegateItem;
			}

			private void Compile()
			{
				if (m_CompiledDelegate != null)
					return;

				ParameterExpression Sender = Expression.Parameter(typeof(object), "sender");
				ParameterExpression Args = Expression.Parameter(typeof(RoutedEventArgs), "args");

				MethodCallExpression MethodCall = Expression.Call(Delegate.Target == null ? null : Expression.Constant(Delegate.Target), Delegate.Method,
					Sender, Expression.Convert(Args, Delegate.Method.GetParameters()[1].ParameterType));

				m_CompiledDelegate = Expression.Lambda<Action<object, RoutedEventArgs>>(MethodCall, Sender, Args).Compile();
			}

			public void Invoke(IRoutedEventsNode owner, RoutedEventArgs args)
			{
				try
				{
					Compile();

					m_CompiledDelegate(owner, args);
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
					throw;
				}
			}
		}

		private readonly SynchronizationContext m_SynchronizationContext;
		private readonly Dictionary<RoutedEvent, FastInvokeDelegateCollection> m_Handlers = new Dictionary<RoutedEvent, FastInvokeDelegateCollection>();

		public IRoutedEventsNode Owner { get; private set; }

		private SynchronizationContext InternalSynchronizationContext
		{
			get { return m_SynchronizationContext ?? SynchronizationHelper.CurrentContext; }
		}

		public RoutedEventSite(IRoutedEventsNode owner, SynchronizationContext synchronizationContext = null)
		{
			m_SynchronizationContext = synchronizationContext;
			Owner = owner;
		}

		public void AddHandler(RoutedEvent routedEvent, Delegate handler)
		{
			if (!m_Handlers.ContainsKey(routedEvent))
				m_Handlers[routedEvent] = new FastInvokeDelegateCollection();

			m_Handlers[routedEvent].Add(handler);
		}

		public void RemoveHandler(RoutedEvent routedEvent, Delegate handler)
		{
			if (!m_Handlers.ContainsKey(routedEvent))
				m_Handlers[routedEvent] = new FastInvokeDelegateCollection();

			m_Handlers[routedEvent].Remove(handler);
		}

		private void InternalInvoke(RoutedEventArgs args)
		{
			InternalSynchronizationContext.Send(
				arg =>
				{
					if (!m_Handlers.ContainsKey(args.RoutedEvent)) return;

					foreach (var Handler in m_Handlers[args.RoutedEvent])
					{
						Handler.Invoke(Owner, args);

						if (args.Handled)
							break;
					}
				},
				null);
		}

		public void RaiseEvent(RoutedEventArgs args)
		{
			switch (args.RoutedEvent.RoutingStrategy)
			{
				case RoutingStrategy.Bubble:
					InternalInvoke(args);

					if (args.Handled)
						return;

					if (Owner.RoutedParent != null)
						Owner.RoutedParent.RoutedSite.RaiseEvent(args);
					break;

				case RoutingStrategy.Tunnel:
					if (Owner.RoutedParent != null)
						Owner.RoutedParent.RoutedSite.RaiseEvent(args);

					if (args.Handled)
						return;

					InternalInvoke(args);
					break;

				case RoutingStrategy.Direct:
					InternalInvoke(args);
					break;

				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}
