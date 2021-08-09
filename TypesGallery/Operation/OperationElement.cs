using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Cryptosoft.TypesGallery.Operation
{
	public class OperationElement : IDisposable, INotifyPropertyChanged
	{
		private readonly LazyProgress<ProgressInfo> m_Progress;
		private string m_Title;
		private double m_Completeness;

		public event PropertyChangedEventHandler PropertyChanged;

		internal event EventHandler Changed;

		public LazyProgress<ProgressInfo> Progress
		{
			get { return m_Progress; }
		}

		public string Title
		{
			get { return m_Title; }
			private set
			{
				m_Title = value;
				OnPropertyChanged();
			}
		}

		public double Completeness
		{
			get { return m_Completeness; }
			private set
			{
				m_Completeness = value;
				OnPropertyChanged();
			}
		}

		public OperationElement(string title = null)
		{
			m_Title = title;
			m_Progress = new LazyProgress<ProgressInfo>(info =>
			{
				if (info.Title != null)
					Title = info.Title;

				if (info.Completeness != null)
					Completeness = (double) info.Completeness;

				if (Changed != null)
					Changed(this, EventArgs.Empty);
			});
		}

		public void Dispose()
		{
			m_Progress.Dispose();
		}

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var Handler = PropertyChanged;
			if (Handler != null) Handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}