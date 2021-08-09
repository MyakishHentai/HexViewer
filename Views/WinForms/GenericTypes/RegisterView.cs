using Cryptosoft.TypesGallery.BaseTypes;
using GeneralViewModel.Interfaces;
using System;

namespace View.WinForms.GenericTypes
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
            Factory.RegisterType<IMainView, ViewForm>();
            return Factory;
        }
    }
}
