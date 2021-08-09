using System;
using System.Threading;
using System.Threading.Tasks;

namespace Cryptosoft.TypesGallery.Operation
{
	public interface IProgressInfo
	{
		bool IsCompleted { get; }
	}

	public class ProgressInfo : IProgressInfo
	{
		public bool IsCompleted
		{
			get { return Completeness >= 1; }
		}

		public double? Completeness { get; private set; }
		public string Title { get; private set; }

		public ProgressInfo(string title, double? completeness)
		{
			Title = title;
			Completeness = completeness;
		}
	}

	public sealed class SimpleProgress<T> : IProgress<T>, IDisposable where T : class, IProgressInfo
	{
		private volatile T m_PreviosProgressInfo;
		private volatile T m_ProgressInfo;

		private readonly Action<T> m_UpdateAction;

		public SimpleProgress() : this(null)
		{
		}

		public SimpleProgress(Action<T> updateAction)
		{
			m_UpdateAction = updateAction;
		}

		internal void ProcessUpdate()
		{
			var ProgressInfo = m_ProgressInfo;

			if (ProgressInfo == m_PreviosProgressInfo) return;

			m_UpdateAction(ProgressInfo);

			m_PreviosProgressInfo = ProgressInfo;
		}

		public void Report(T value)
		{
			m_ProgressInfo = value;

			ProcessUpdate();
		}

		public void Dispose()
		{
		}
	}

	public sealed class LazyProgress<T> : IProgress<T>, IDisposable where T : class, IProgressInfo
	{
		private volatile T m_PreviosProgressInfo;
		private volatile T m_ProgressInfo;

		private readonly Action<T> m_UpdateAction;

		private readonly CancellationTokenSource m_CancellationTokenSource = new CancellationTokenSource();

		public LazyProgress() : this(null)
		{
		}

		public LazyProgress(Action<T> updateAction)
		{
			m_UpdateAction = updateAction;

			var CancellationToken = m_CancellationTokenSource.Token;

			Task.Factory.StartNew(async () =>
			{
				while (!CancellationToken.IsCancellationRequested)
				{
					await Task.Delay(150);
					ProcessUpdate();
				}
			});
		}

		internal void ProcessUpdate()
		{
			var ProgressInfo = m_ProgressInfo;

			if (ProgressInfo == m_PreviosProgressInfo) return;

			m_UpdateAction(ProgressInfo);

			m_PreviosProgressInfo = ProgressInfo;
		}

		public void Report(T value)
		{
			m_ProgressInfo = value;
		}

		public void Dispose()
		{
			m_CancellationTokenSource.Cancel();
		}
	}

}