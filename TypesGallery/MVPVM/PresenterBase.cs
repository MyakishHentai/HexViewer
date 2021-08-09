using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Cryptosoft.TypesGallery.BaseTypes;
using Cryptosoft.TypesGallery.Events;

namespace Cryptosoft.TypesGallery.MVPVM
{
	/// <summary>
	/// Базовый класс презентатора
	/// </summary>
	public abstract class PresenterBase<TView, TViewModel, TBusinesLogicLayer> : IPresenter, INotifyPropertyChanged
		where TView : class, IView<TViewModel>
		where TViewModel : class, IViewModel
		where TBusinesLogicLayer : class, IBusinesLogicLayer
		
	{
		private TViewModel m_ViewModel;
		private TView m_View;
		private bool m_IsViewClosed = true;
		private readonly object m_SynchronizationObject = new object();
		private readonly object m_CloseSynchronizationObject = new object();
		private IDisposable m_CurrentRunDisposable;
		private IPresenter m_Parent;
		private IDesktopLifetime m_Lifetime;
		protected TypesFactoryAgregator FactoryAgregator { get; private set; }


		public bool StopRunOnViewClosed { get; protected set; }

		public bool SwapView { get; protected set; }

		public bool SwapViewModel { get; protected set; }

		/// <summary>
		/// Родительский презентатор
		/// </summary>
		public IPresenter Parent
		{
			get { return m_Parent; }
			protected set
			{
				if (m_Parent == value)
					return;

				if (m_Parent != null) m_Parent.Childs.Remove(this);

				m_Parent = value;

				if (m_Parent != null) m_Parent.Childs.Add(this);
			}
		}

		/// <summary>
		/// Контроль отображения, настройка конфигурации
		/// </summary>
		public IDesktopLifetime Lifetime
		{
			get { return m_Lifetime; }
			protected set
			{
				if (m_Lifetime == value)
					return;

				m_Lifetime = value;
			}
		}

		/// <summary>
		/// Фабрика
		/// </summary>
		public ITypesFactory Factory
		{
			get { return FactoryAgregator; }
		}

		/// <summary>
		/// Модель представления
		/// </summary>
		public TViewModel ViewModel
		{
			get { return m_ViewModel; }
			protected set
			{
				m_ViewModel = value;
				OnPropertyChanged();
			}
		}

		public ICollection<IPresenter> Childs { get; protected set; }

		protected virtual void InitializeLifetime()
		{
			if (Lifetime == null)
				Lifetime = Factory.Get<IDesktopLifetime>();
		}

		public void Stop()
		{
			if (!CanStop)
				return;

			if (!OnStopping())
				return;

			IDisposable OldValue = Interlocked.Exchange(ref m_CurrentRunDisposable, null);
			if (OldValue != null) OldValue.Dispose();

			OnStopped();
		}

		protected virtual bool OnStopping()
		{
			return true;
		}

		protected virtual void OnStopped()
		{ }

		/// <summary>
		/// Представление
		/// </summary>
		public TView View
		{
			get { return m_View; }
			protected set
			{
				m_View = value;
				OnPropertyChanged();
			}
		}

		ITypesFactoryOwner ITypesFactoryOwner.Parent
		{
			get { return Parent; }
		}

		protected PresenterBase(IPresenter parent = null)
		{
			FactoryAgregator = new TypesFactoryAgregator(parent == null ? null : parent.Factory);
			StopRunOnViewClosed = true;
			Parent = parent;
			Childs = new List<IPresenter>();
		}

		/// <summary>
		/// Функция выполнения презентатора
		/// </summary>
		/// <param name="routedParent"></param>
		public void Run(IRoutedEventsNode routedParent = null)
		{
			lock (m_SynchronizationObject)
			{
				if (m_CurrentRunDisposable != null)
					throw new InvalidOperationException("Презентатор уже запущен.");

				m_CurrentRunDisposable = Disposable.Create(FreeRun);				
				
				RunViewModel(routedParent);
				RunView();
				OnRun();
			}
			Lifetime?.Activate(null);
		}

		private void ViewOnViewClosed(object sender, ViewClosedEventArgs viewClosedEventArgs)
		{
			lock (m_CloseSynchronizationObject)
			{
				if (!Equals(sender, View)) return;

				bool AlreadyClosed = m_IsViewClosed;
				m_IsViewClosed = true;

				if (!AlreadyClosed && ViewClosed != null)
					ViewClosed(this, EventArgs.Empty);

				if (StopRunOnViewClosed)
					Stop();
			}
		}

		protected virtual bool CanStopInternal { get { return true; } }

		public bool CanStop
		{
			get { return CanStopInternal && (ViewModel == null || ViewModel.CanStop) && Childs.All(child => child.CanStop); }
		}

		private void FreeRun()
		{
			lock (m_SynchronizationObject)
			{
				foreach (IPresenter Child in Childs)
				{
					Child.Stop();
				}

				if (!m_IsViewClosed)
				{
					if (View != null) View.Close();
				}

				if (!SwapView)
				{
					IDisposable OldValue = Interlocked.Exchange(ref m_View, null);
					if (OldValue != null) OldValue.Dispose();
					OnPropertyChangedEx("View");
				}


				m_ViewModel.OnStop();

				if (!SwapViewModel)
				{
					Interlocked.Exchange(ref m_ViewModel, null);
					OnPropertyChangedEx("ViewModel");
				}

				OnRunStopped();
			}
		}

		protected virtual void RunViewModel(IRoutedEventsNode routedParent)
		{
			if (ViewModel == null)
			{
				ViewModel = Factory.Get<TViewModel>();
				ViewModel.Init(routedParent ?? (Parent != null ? Parent.ViewModel : null), Factory);
			}

			ViewModel.Content = Factory.Get<TBusinesLogicLayer>();
			ViewModel.Run();
		}

		protected virtual void RunView()
		{
			m_IsViewClosed = false;

			SynchronizationHelper.ClearCurrentSynchronizationContext();

			if (View == null)
			{
				View = Factory.Get<TView>();
				Lifetime?.SetInstance(View);
				View.ViewClosed += ViewOnViewClosed;
			}
			SynchronizationHelper.UpdateCurrentSynchronizationContext();

			View.Model = ViewModel;

			View.Activate(Parent != null ? Parent.View : null);
		}

		public event EventHandler RunStopped;
		public event EventHandler ViewClosed;

		private void OnRunStopped()
		{
			if (RunStopped != null) RunStopped.Invoke(this, EventArgs.Empty);
		}

		IView IPresenter.View
		{
			get { return View; }
		}

		IViewModel IPresenter.ViewModel
		{
			get { return ViewModel; }
		}

		protected virtual void OnRun()
		{
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			if (PropertyChanged != null) PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		protected virtual void OnPropertyChangedEx(string propertyName)
		{
			if (PropertyChanged != null) PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public PresenterBase<TView, TViewModel, TBusinesLogicLayer> ExtendFactory(TypesFactory factory)
		{
			FactoryAgregator.Add(factory);
			return this;
		}

		protected virtual void OnRefresh()
		{
		}

		public void Refresh()
		{
			if (ViewModel != null)
				ViewModel.Refresh();

			OnRefresh();
		}
	}

	public abstract class NoViewPresenterBase : IPresenter, INotifyPropertyChanged
	{
		private readonly object m_SynchronizationObject = new object();
		private IDisposable m_CurrentRunDisposable;
		private IPresenter m_Parent;
		protected TypesFactoryAgregator FactoryAgregator { get; private set; }


		public bool StopRunOnViewClosed { get; protected set; }

		/// <summary>
		/// Родительский презентатор
		/// </summary>
		public IPresenter Parent
		{
			get { return m_Parent; }
			protected set
			{
				if (m_Parent == value)
					return;

				if (m_Parent != null) m_Parent.Childs.Remove(this);

				m_Parent = value;

				if (m_Parent != null) m_Parent.Childs.Add(this);
			}
		}

		/// <summary>
		/// Фабрика
		/// </summary>
		public ITypesFactory Factory
		{
			get { return FactoryAgregator; }
		}

		/// <summary>
		/// Модель представления
		/// </summary>
		public IViewModel ViewModel
		{
			get { return null; }
		}

		public ICollection<IPresenter> Childs { get; private set; }

		public void Stop()
		{
			if (!CanStop)
				return;

			if (!OnStopping())
				return;

			IDisposable OldValue = Interlocked.Exchange(ref m_CurrentRunDisposable, null);
			if (OldValue != null) OldValue.Dispose();

			OnStopped();
		}

		protected virtual bool OnStopping()
		{
			return true;
		}

		protected virtual void OnStopped()
		{ }

		/// <summary>
		/// Представление
		/// </summary>
		public IView View
		{
			get { return null; }
		}

		ITypesFactoryOwner ITypesFactoryOwner.Parent
		{
			get { return Parent; }
		}

		protected NoViewPresenterBase(IPresenter parent = null)
		{
			FactoryAgregator = new TypesFactoryAgregator(parent == null ? null : parent.Factory);
			StopRunOnViewClosed = true;
			Parent = parent;
			Childs = new List<IPresenter>();
		}

		/// <summary>
		/// Функция выполнения презентатора
		/// </summary>
		/// <param name="routedParent"></param>
		public void Run(IRoutedEventsNode routedParent = null)
		{
			lock (m_SynchronizationObject)
			{
				if (m_CurrentRunDisposable != null)
					throw new InvalidOperationException("Презентатор уже запущен.");

				m_CurrentRunDisposable = Disposable.Create(FreeRun);

				OnRun();
			}
		}

		public void Refresh()
		{
			
		}

		protected virtual bool CanStopInternal { get { return true; } }

		public bool CanStop
		{
			get { return CanStopInternal && Childs.All(child => child.CanStop); }
		}

		private void FreeRun()
		{
			lock (m_SynchronizationObject)
			{
				foreach (IPresenter Child in Childs)
				{
					Child.Stop();
				}

				OnRunStopped();
			}
		}

		public event EventHandler RunStopped;

		private void OnRunStopped()
		{
			if (RunStopped != null) RunStopped.Invoke(this, EventArgs.Empty);
		}

		protected virtual void OnRun()
		{
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			if (PropertyChanged != null) PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		protected virtual void OnPropertyChangedEx(string propertyName)
		{
			if (PropertyChanged != null) PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public NoViewPresenterBase ExtendFactory(TypesFactory factory)
		{
			FactoryAgregator.Add(factory);
			return this;
		}
	}
}