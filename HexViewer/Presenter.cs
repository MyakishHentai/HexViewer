using Cryptosoft.TypesGallery.MVPVM;
using GeneralViewModel.Interfaces;
using HexViewer.CoreLogic;

namespace HexViewer
{
    internal class Presenter : PresenterBase<IMainView, IMainViewModel, IBusinesLogicLayer>
    {
        public Presenter()
        {
            Bll = new ReadingFile();
            FactoryAgregator.RegisterSingleton(Bll);
        }

        /// <summary>
        ///     Бизнеслогика.
        /// </summary>
        private IBusinesLogicLayer Bll { get; }


        /// <summary>
        ///     Инициализация Lifetime.
        /// </summary>
        public void SetLifitime()
        {
            InitializeLifetime();
        }
    }
}