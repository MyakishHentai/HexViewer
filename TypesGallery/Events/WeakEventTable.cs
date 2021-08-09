using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Cryptosoft.TypesGallery.BaseTypes;

namespace Cryptosoft.TypesGallery.Events
{
	public abstract class WeakEventTable<T, TArguments> where T : class
	{
		readonly Hashtable m_Table = new Hashtable();

		private static readonly object s_StaticSource = new object();
		private int m_CleanupRequests;

		struct Listener
		{
			private readonly WeakReference m_Target;
			private readonly WeakReference m_Handler;

			public Listener(object target, Delegate handler)
			{
				m_Target = new WeakReference(target);
				m_Handler = new WeakReference(handler);
			}

			public bool Matches(object target, Delegate handler)
			{
				return ReferenceEquals(target, Target) && Equals(handler, Handler);
			}

			public Delegate Handler
			{
				get { return (Delegate) m_Handler.Target; }
			}

			public object Target
			{
				get { return m_Target.Target; }
			}
		}

		protected class ListenerList
		{
			private static readonly ListenerList s_Empty = new ListenerList();

			private readonly List<Listener> m_List = new List<Listener>();

			private readonly ConditionalWeakTable<object, object> m_Table = new ConditionalWeakTable<object, object>();
			private int m_Users;

			public bool IsEmpty
			{
				get { return m_List.Count == 0; }
			}

			public static ListenerList Empty
			{
				get { return s_Empty; }
			}

			public void AddHandler(Delegate handler)
			{
				ChechForUsing();

				object Target = handler.Target ?? s_StaticSource;

				m_List.Add(new Listener(Target, handler));

				object Value;
				if (!m_Table.TryGetValue(Target, out Value))
				{
					m_Table.Add(Target, handler);
				}
				else
				{
					var Delegates = Value as List<Delegate>;

					if (Delegates == null)
					{
						Delegates = new List<Delegate> {Value as Delegate};

						m_Table.Remove(Target);
						m_Table.Add(Target, Delegates);
					}

					Delegates.Add(handler);
				}
			}

			public void RemoveHandler(Delegate handler)
			{
				ChechForUsing();

				object Target = handler.Target ?? s_StaticSource;

				for (int i = m_List.Count - 1; i >= 0; i--)
				{
					if (m_List[i].Matches(Target, handler))
					{
						m_List.RemoveAt(i);
						break;
					}
				}

				object Value;
				if (m_Table.TryGetValue(Target, out Value))
				{
					var Delegates = Value as List<Delegate>;

					if (Delegates == null)
					{
						m_Table.Remove(Target);
					}
					else
					{
						Delegates.Remove(handler);
						if (Delegates.Count == 0)
						{
							m_Table.Remove(Target);
						}
					}
				}
			}

			public bool DeliverEvent(object sender, TArguments args, Action<T, object, TArguments> invokeEvent)
			{
				bool FoundStaleEntries = false;

				for (int i = 0, n = m_List.Count; i < n; i++)
				{
					var Listener = m_List[i];

					object Target = Listener.Target;
					bool EntryIsStale = (Target == null);

					if (!EntryIsStale)
					{
						var Handler = Listener.Handler as T;

						if (Handler != null)
						{
							invokeEvent(Handler, sender, args);
						}
					}

					FoundStaleEntries |= EntryIsStale;
				}

				return FoundStaleEntries;
			}

			public static bool PrepareForWriting(ref ListenerList list)
			{
				bool InUse = list.BeginUse();
				list.EndUse();

				if (InUse)
				{
					list = list.Clone();
				}

				return InUse;
			}

			public bool Purge()
			{
				ChechForUsing();

				bool FoundDirt = false;

				for (int i = m_List.Count - 1; i >= 0; i--)
				{
					if (m_List[i].Target == null)
					{
						m_List.RemoveAt(i);
						FoundDirt = true;
					}
				}

				return FoundDirt;
			}

			void ChechForUsing()
			{
				if (m_Users != 0)
					throw new InvalidOperationException();
			}

			ListenerList Clone()
			{
				ListenerList NewList = new ListenerList();

				for (int i = 0, n = m_List.Count; i < n; i++)
				{
					Listener Lst = m_List[i];
					if (Lst.Target != null)
					{
						Delegate Handler = Lst.Handler;
						if (Handler != null)
						{
							NewList.AddHandler(Handler);
						}
					}
				}

				return NewList;
			}

			public bool BeginUse()
			{
				return Interlocked.Increment(ref m_Users) != 1;
			}

			public void EndUse()
			{
				Interlocked.Decrement(ref m_Users);
			}
		}


		protected void ProtectedAddListener(object source, Delegate handler)
		{
			object sourceKey = new TableKey(source);

			ListenerList Listeners;

			lock (m_Table)
			{
				Listeners = m_Table.ContainsKey(sourceKey) ? (ListenerList) m_Table[sourceKey] : null;

				if (Listeners == null)
				{
					Listeners = new ListenerList();
					m_Table.Add(sourceKey, Listeners);

					StartListening(source);
				}

				if (ListenerList.PrepareForWriting(ref Listeners))
				{
					m_Table[sourceKey] = Listeners;
				}
			}

			Listeners.AddHandler(handler);

			Cleanup();
		}

		protected void ProtectedRemoveListener(object source, Delegate handler)
		{
			object sourceKey = new TableKey(source);

			lock (m_Table)
			{
				var Listeners = m_Table.ContainsKey(sourceKey) ? (ListenerList) m_Table[sourceKey] : null;

				if (Listeners != null)
				{
					if (ListenerList.PrepareForWriting(ref Listeners))
					{
						m_Table[sourceKey] = Listeners;
					}

					Listeners.RemoveHandler(handler);

					if (Listeners.IsEmpty)
					{
						m_Table.Remove(sourceKey);
						StopListening(source);
					}
				}
			}
		}

		protected void DeliverEvent(object sender, TArguments args)
		{
			object sourceKey = new TableKey(sender);

			ListenerList Listeners;

			lock (m_Table)
			{
				Listeners = m_Table.ContainsKey(sourceKey) ? (ListenerList) m_Table[sourceKey] : ListenerList.Empty;
			}

			Listeners.BeginUse();

			try
			{
				bool FoundStaleEntries = Listeners.DeliverEvent(sender, args, InvokeEvent);

				if (FoundStaleEntries)
					Cleanup();
			}
			finally
			{
				Listeners.EndUse();
			}
		}

		private void Cleanup()
		{
			if (Interlocked.Increment(ref m_CleanupRequests) == 1)
			{
				Dispatcher.Current.Add(ExecutionPriority.Minimum, o => CleanupOperation(), null);
			}
		}

		private void CleanupOperation()
		{
			Interlocked.Exchange(ref m_CleanupRequests, 0);

			Purge();
		}

		protected abstract void StartListening(object source);

		protected abstract void StopListening(object source);

		protected abstract void InvokeEvent(T handle, object sender, TArguments args);

		private void Purge()
		{
			lock (m_Table)
			{
				ICollection KeysCollection = m_Table.Keys;

				TableKey[] Keys = new TableKey[KeysCollection.Count];

				KeysCollection.CopyTo(Keys, 0);

				for (int i = Keys.Length - 1; i >= 0; i--)
				{
					object Data = m_Table[Keys[i]];

					if (Data != null)
					{
						object Source = Keys[i].Source;

						bool RemoveList = Source == null;

						if (!RemoveList)
						{
							ListenerList List = (ListenerList) Data;

							if (ListenerList.PrepareForWriting(ref List))
							{
								m_Table[Keys[i]] = List;
							}

							List.Purge();

							RemoveList = List.IsEmpty;
						}

						if (RemoveList)
						{
							if (Source != null)
							{
								StopListening(Source);

								m_Table.Remove(Keys[i]);
							}
						}

						if (Source == null)
						{
							m_Table.Remove(Keys[i]);
						}
					}
				}
			}
		}

		class TableKey
		{
			private readonly int m_Hash;
			private readonly WeakReference m_Reference;

			public object Source
			{
				get { return m_Reference.Target; }
			}

			public TableKey(object source)
			{
				m_Reference = new WeakReference(source ?? s_StaticSource);
				m_Hash = source.GetHashCode();
			}

			public override int GetHashCode()
			{
				return m_Hash;
			}

			public override bool Equals(object obj)
			{
				var Obj = obj as TableKey;

				return Obj != null && Equals(Source, Obj.Source);
			}
		}
	}
}