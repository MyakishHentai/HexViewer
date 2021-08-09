using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;
using Cryptosoft.TypesGallery.MVPVM;

namespace View.AvaloniaUI   
{
    public class BuildApp : IDesktopLifetime
    {
        private readonly ClassicDesktopStyleApplicationLifetime m_LifetimeMode;

        public BuildApp()
        {
            var AppBuilder = BuildAvaloniaApp();
            m_LifetimeMode = new ClassicDesktopStyleApplicationLifetime {ShutdownMode = ShutdownMode.OnMainWindowClose};
            AppBuilder.SetupWithLifetime(m_LifetimeMode);
        }

        /// <summary>
        ///     Связывание экземпляра с отображением.
        /// </summary>
        /// <param name="Window">Экземпляр класса отображаемого окна <see cref="IView" /></param>
        public void SetInstance(IView Window)
        {
            m_LifetimeMode.MainWindow = Window as Window;
        }

        /// <summary>
        ///     Запуск приложения.
        /// </summary>
        /// <param name="args">Параметры запуска.</param>
        public void Activate(string[] args)
        {
            m_LifetimeMode.Start(args);
        }

        /// <summary>
        ///     The entry point. Things aren't ready yet, so at this point
        ///     you shouldn't use any Avalonia types or anything that expects
        ///     a SynchronizationContext to be ready.
        /// </summary>
        /// <returns>Initializes a new instance of the Avalonia.AppBuilder class.</returns>
        public static AppBuilder BuildAvaloniaApp()
        {
            return AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace()
                .UseReactiveUI();
        }
    }
}