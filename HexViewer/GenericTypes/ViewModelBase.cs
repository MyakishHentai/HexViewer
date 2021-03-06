using System;
using System.Threading.Tasks;
using Cryptosoft.TypesGallery.BaseTypes;
using Cryptosoft.TypesGallery.Commands;
using Cryptosoft.TypesGallery.Events;
using Cryptosoft.TypesGallery.MVPVM;
using ReactiveUI;

namespace HexViewer.GenericTypes
{
    public abstract class ViewModelBase : ReactiveObject, IViewModel
    {
        private IBusinesLogicLayer m_Content;
        private bool m_Locked;

        protected ViewModelBase()
        {
            RoutedSite = new RoutedEventSite(this);
            CommandSite = new CommandTargetSite();
        }

        protected ITypesFactory Factory { get; private set; }
        public IRoutedEventsNode RoutedParent { get; private set; }
        public RoutedEventSite RoutedSite { get; }
        public CommandTargetSite CommandSite { get; }

        public bool Locked
        {
            get => m_Locked;
            set => this.RaiseAndSetIfChanged(ref m_Locked, value);
        }

        public virtual bool CanStop => true;

        public virtual bool TryStop()
        {
            return true;
        }

        public virtual Task<bool> TryStopAsync()
        {
            return Task.Factory.StartNew(() => TryStop());
        }

        public IBusinesLogicLayer Content
        {
            get => m_Content;
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