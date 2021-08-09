using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Cryptosoft.TypesGallery.BaseTypes
{
	public class Dispatcher : IDisposable
	{
		private static readonly Lazy<Dispatcher> s_Current = new Lazy<Dispatcher>(LazyThreadSafetyMode.ExecutionAndPublication);

		private readonly Thread m_WorkThread;

		private readonly Queue<Tuple<Action<object>, object>> m_Actions = new Queue<Tuple<Action<object>, object>>();

		private readonly AutoResetEvent m_Event = new AutoResetEvent(false);

		public static Dispatcher Current
		{
			get { return s_Current.Value; }
		}

		public Dispatcher()
		{
			m_WorkThread = new Thread(Start)
			{
				IsBackground = true,
				Priority = ThreadPriority.Lowest
			};
			m_WorkThread.Start();
		}

		private void Start()
		{
			while (true)
			{
				m_Event.WaitOne();

				while (m_Actions.Count != 0)
				{
					Thread.Sleep(60);

					var Item = m_Actions.Dequeue();

					Item.Item1.Invoke(Item.Item2);
				}
			}
			// ReSharper disable once FunctionNeverReturns
			// Функция выполняется в фоновом потоке и прерывается при завершении приложения
		}

		public void Add(ExecutionPriority priority, Action<object> action, object parameter)
		{
			m_Actions.Enqueue(new Tuple<Action<object>, object>(action, parameter));
			m_Event.Set();
		}

		public void Dispose()
		{
			m_WorkThread.Abort();
		}
	}

	public enum ExecutionPriority
	{
		Maximum,
		Normal,
		Minimum,
	}

	public class DispatcherEx : IDisposable
	{
		private readonly Thread m_WorkThread;

		private readonly ConcurrentQueue<TaskInfo> m_Actions = new ConcurrentQueue<TaskInfo>();
		private readonly SortedDictionary<DateTime, TaskInfo> m_SuspendActions = new SortedDictionary<DateTime, TaskInfo>();

		private readonly AutoResetEvent m_Event = new AutoResetEvent(false);
		private readonly AutoResetEvent m_SuspendEvent = new AutoResetEvent(false);
		private readonly ManualResetEvent m_StopEvent = new ManualResetEvent(false);
		private DateTime m_NextActionIn;
		private TimeSpan m_Accuracy = TimeSpan.FromMilliseconds(10);
		private bool m_Disposed;

		public DispatcherEx(string name = "DispatcherEx")
		{
			m_WorkThread = new Thread(Start)
			{
				Name = name,
				IsBackground = true,
				Priority = ThreadPriority.Lowest
			};

			m_WorkThread.Start();
		}

		private void Start()
		{
			while (true)
			{
				int Timeout = System.Threading.Timeout.Infinite;

				DateTime NowTime = DateTime.UtcNow;

				if (NowTime < m_NextActionIn)
				{
					Timeout = (int)(m_NextActionIn - NowTime).TotalMilliseconds;
				}
				else if (m_SuspendActions.Count > 0)
				{
					Timeout = (int)Accuracy.TotalMilliseconds;
				}

				switch (WaitHandle.WaitAny(new WaitHandle[] { m_Event, m_SuspendEvent, m_StopEvent }, Timeout))
				{
					case 0:
						// Есть операции готовые к выполнению
						Execute();
						break;

					case 1:
					case WaitHandle.WaitTimeout:
						// Пришло время отложенной операции или нужно обновить время ожидания
						ReScheduleAfterSuspend();
						continue;

					default:
						// Завершаем поток
						return;
				}
			}
		}

		public void Schedule(TimeSpan suspendTime, Action<object> action, object parameter)
		{
			TaskInfo Info = new TaskInfo
			{
				Action = action,
				Argument = parameter,
				ExecutionDate = DateTime.UtcNow + suspendTime,
			};

			if (suspendTime <= Accuracy)
			{
				ScheduleInternal(Info);
			}
			else
			{
				ScheduleSuspendedInternal(Info);
			}
		}

		void ScheduleInternal(TaskInfo actionInfo)
		{
			m_Actions.Enqueue(actionInfo);
			m_Event.Set();
		}

		void ScheduleSuspendedInternal(TaskInfo actionInfo)
		{
			DateTime NewTime = actionInfo.ExecutionDate;

			lock (m_SuspendActions)
			{
				while (m_SuspendActions.ContainsKey(NewTime))
					NewTime = NewTime.AddTicks(1);

				m_SuspendActions[NewTime] = actionInfo;
			}

			m_SuspendEvent.Set();
		}

		public TimeSpan Accuracy
		{
			get { return m_Accuracy; }
			set { m_Accuracy = value; }
		}

		private readonly List<DateTime> m_Remove = new List<DateTime>();

		void ReScheduleAfterSuspend()
		{
			m_Remove.Clear();

			lock (m_SuspendActions)
			{
				var Now = DateTime.UtcNow - Accuracy;

				foreach (var SuspendAction in m_SuspendActions)
				{
					if (SuspendAction.Key < Now)
					{
						m_Remove.Add(SuspendAction.Key);
						ScheduleInternal(SuspendAction.Value);
					}
					else
					{
						if (m_NextActionIn != SuspendAction.Key)
						{
							m_NextActionIn = SuspendAction.Key;
							m_SuspendEvent.Set();
						}

						break;
					}
				}

				foreach (var Date in m_Remove)
				{
					m_SuspendActions.Remove(Date);
				}
			}
		}

		void Execute()
		{
			TaskInfo Item;

			while (m_Actions.TryDequeue(out Item))
			{
				if (m_StopEvent.WaitOne(0))
					return;

				Item.Action.Invoke(Item.Argument);
			}
		}

		public void Stop()
		{
			m_StopEvent.Set();
			m_WorkThread.Join();
		}

		~DispatcherEx()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
		}

		private void Dispose(bool disposing)
		{
			if (m_Disposed)
				return;

			m_Disposed = true;

			Stop();

			if (disposing)
			{
				m_StopEvent.Dispose();
				m_SuspendEvent.Dispose();
				m_Event.Dispose();
			}
		}
	}

	internal class TaskInfo
	{
		public Action<object> Action { get; set; }
		public object Argument { get; set; }
		public DateTime ExecutionDate { get; set; }
	}
}