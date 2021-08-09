using System;
using Avalonia;
using Cryptosoft.TypesGallery.BaseTypes;
using View.AvaloniaUI.Views;
using GeneralViewModel.Interfaces;
using Cryptosoft.TypesGallery.MVPVM;

namespace View.AvaloniaUI.GenericTypes
{
    public static class RegisterLifetime
    {
        /// <summary>
        ///     Регистрация класса, отвечающего за Lifetime.
        /// </summary>
        /// <param name="currentOption">Выбранный способ отображения.</param>
        /// <returns></returns>
        public static TypesFactory Create()
        {
            var Factory = new TypesFactory();
                                   
            Factory.RegisterType<IDesktopLifetime, BuildApp>();

            return Factory;
        }
    }
}
