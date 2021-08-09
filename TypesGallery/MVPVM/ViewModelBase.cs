using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Cryptosoft.TypesGallery.BaseTypes;
using Cryptosoft.TypesGallery.Commands;
using Cryptosoft.TypesGallery.Events;

namespace Cryptosoft.TypesGallery.MVPVM
{
	public abstract class ViewModelBase : IViewModel, INotifyPropertyChanged
	{
		private IBusinesLogicLayer m_Content;
		private bool m_Locked;
		public IRoutedEventsNode RoutedParent { get; private set; }
		public RoutedEventSite RoutedSite { get; private set; }
		public CommandTargetSite CommandSite { get; private set; }
		public bool Locked
		{
			get { return m_Locked; }
			set
			{
				m_Locked = value;
				OnPropertyChanged();
			}
		}

		public virtual bool CanStop { get { return true; } }

		public virtual bool TryStop()
		{
			return true;
		}

		public virtual Task<bool> TryStopAsync()
		{
			return Task.Factory.StartNew(() => TryStop());
		}

		protected ITypesFactory Factory { get; private set; }

		public IBusinesLogicLayer Content
		{
			get { return m_Content; }
			set
			{
				if (m_Content != value)
				{
					m_Content = value;

					OnContentChanged();
				}
			}
		}

		public void Init(IRoutedEventsNode routedParent, ITypesFactory factory)
		{
			Factory = factory;
			RoutedParent = routedParent;
			OnInit();
		}

		public void Run()
		{
			OnRun();
		}

		public virtual void OnStop()
		{
		}

		public void Refresh()
		{
			OnRefresh();
		}

		protected virtual void OnRefresh()
		{
		}

		protected virtual void OnInit()
		{

		}

		protected virtual void OnRun()
		{

		}

		protected virtual void OnContentChanged()
		{

		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			if (PropertyChanged != null) PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		protected ViewModelBase()
		{
			RoutedSite = new RoutedEventSite(this);
			CommandSite = new CommandTargetSite();
		}
	}

	public static class ViewModelHelper
	{
		public static IDisposable Lock(this IViewModel viewModel)
		{
			viewModel.Locked = true;
			return Disposable.Create(() => viewModel.Locked = false);
		}
	}
}