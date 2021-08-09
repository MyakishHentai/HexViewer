using System;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.InteropServices;
using HexViewer.GenericTypes;
using AvaloniaUI = View.AvaloniaUI;
using WF = View.WinForms;

namespace HexViewer
{
    /// <summary>
    ///     Последовательноть вариантов отображения.
    /// </summary>
    public enum ViewOption
    {
        AvaloniaUI = 1,
        WinForms = 2
    }

    internal class Program
    {
        public static ApplicationOptionsSection Options { get; private set; }

        [STAThread]
        /// <summary>
        ///     Точка входа, определяющая вид отображения (WinForm/AvaloniaUI) и задающая начальную конфигурацию.
        /// </summary>
        /// <param name="args">Параметры, передаваемые при входе</param>
        private static void Main(string[] args)
        {
            var Config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            Options = Config.GetSection("ApplicationOptions") as ApplicationOptionsSection ??
                      new ApplicationOptionsSection();
            CheckView();
            Debug.WriteLine("Stop working");
        }

        /// <summary>
        ///     Проверка конфигурации => Выбор способа отображения, регистрация типов в Factory
        ///     Запуск Presenter.
        /// </summary>
        private static void CheckView()
        {
            var MainPresenter = new Presenter();
            MainPresenter.ExtendFactory(RegisterViewModel.Create());

            switch (Options.ViewsOptions.CurrentView)
            {
                case ViewOption.AvaloniaUI:
                    Debug.WriteLine("Используется отображене AvaloniaUI!");
                    MainPresenter.ExtendFactory(AvaloniaUI.GenericTypes.RegisterLifetime.Create());
                    MainPresenter.SetLifitime();
                    MainPresenter.ExtendFactory(AvaloniaUI.GenericTypes.RegisterView.Create());
                    #region // DLL:

                    //string Path = "View.AvaloniaUI.dll";
                    //Assembly Ass = Assembly.LoadFrom(Path);
                    //Type CustomerType = Ass.GetType("View.AvaloniaUI.GenericTypes.RegisterView");
                    //MethodInfo StaticMethod = CustomerType.GetMethod("Create");
                    //MainPresenter.ExtendFactory((TypesFactory)StaticMethod.Invoke(null, null));

                    #endregion
                    break;

                case ViewOption.WinForms:
                    Debug.WriteLine("Используется отображене WinForms!");
                    WF.BuildApp.TextRendering();
                    MainPresenter.ExtendFactory(WF.GenericTypes.RegisterView.Create());
                    break;

                default:
                    Debug.WriteLine("Configuration error!");
                    AllocConsole();
                    Console.WriteLine("--- Ошибка конфигурации ---\n" +
                                      "Некорректное значение в файле конфигурации приложения!\n" +
                                      "Проверьте параметр ViewOption - ApplicationOptionsSection.cs");
                    Console.ReadKey();
                    return;
            }

            MainPresenter.Run();
        }

        [DllImport("kernel32.dll")]
        static extern bool AllocConsole();
    }
}