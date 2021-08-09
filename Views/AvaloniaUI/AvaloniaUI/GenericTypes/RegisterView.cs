using Cryptosoft.TypesGallery.BaseTypes;
using GeneralViewModel.Interfaces;
using View.AvaloniaUI.Views;

namespace View.AvaloniaUI.GenericTypes
{
    public static class RegisterView
    {
        /// <summary>
        ///     Регистрация View.
        /// </summary>
        /// <param name="currentOption">Выбранный способ отображения.</param>
        /// <returns></returns>
        public static TypesFactory Create()
        {
            var Factory = new TypesFactory();
            Factory.RegisterType<IMainView, MainWindow>();
            return Factory;
        }
    }
}