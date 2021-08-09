using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using Cryptosoft.TypesGallery.MVPVM;
using GeneralViewModel.Interfaces;

namespace View.AvaloniaUI.Views
{
    public class MainWindow : Window, IMainView
    {
        private TextBlock m_HexBlock;

        private IMainViewModel m_Model;

        public MainWindow()
        {
            InitializeComponent();
            //m_HexBlock.LineHeight = 20.0;
#if DEBUG
            //this.AttachDevTools();
#endif
        }

        public event EventHandler<ViewClosedEventArgs> ViewClosed;

        public IMainViewModel Model
        {
            get => DataContext as IMainViewModel;
            set
            {
                if (m_Model == value)
                    return;
                DataContext = value;
            }
        }

        public object DataSource
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public void Activate(IView parentView)
        {
        }

        public void OnHide()
        {
        }

        public void Dispose()
        {
        }


        /// <summary>
        ///     Асинхронная операция для OpenFileDialog().
        /// </summary>
        /// <returns>Директория файла.</returns>
        public async Task<string> GetPath()
        {
            var Window = this?.GetVisualRoot() as Window;
            if (Window == null)
                return null;
            var Dialog = new OpenFileDialog();
            var Result = await Dialog.ShowAsync(Window);

            if (Result == null)
                await GetPath();
            return Result?.FirstOrDefault();
        }


        /// <summary>
        ///     Выбор нового файла для сравнения.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public async void OnOpenClicked(object sender, RoutedEventArgs args)
        {
            var Path = await GetPath();
            if (Path != null) Model.FilePath = Path;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            #region // SizeChanged:

            m_HexBlock = this.FindControl<TextBlock>("HexBlock");
            m_HexBlock.GetPropertyChangedObservable(BoundsProperty)
                .Subscribe(args =>
                {
                    //m_HexBlock.Measure(new Avalonia.Size(double.PositiveInfinity, double.PositiveInfinity));
                    Debug.WriteLine("Размеры изменены");
                });

            #endregion
        }
    }
}