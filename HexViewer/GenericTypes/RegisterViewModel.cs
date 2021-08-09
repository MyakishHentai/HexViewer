using Cryptosoft.TypesGallery.BaseTypes;
using GeneralViewModel.Interfaces;

namespace HexViewer.GenericTypes
{
    public class RegisterViewModel
    {
        /// <summary>
        ///     Регистрация View.
        /// </summary>
        /// <param name="currentOption">Выбранный способ отображения.</param>
        /// <returns></returns>
        public static TypesFactory Create()
        {
            var Factory = new TypesFactory();
            Factory.RegisterType<IMainViewModel, ViewModel>();
            return Factory;
        }
    }
}