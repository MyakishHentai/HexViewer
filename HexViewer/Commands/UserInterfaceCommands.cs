using Cryptosoft.TypesGallery.Commands;
//using View.AvaloniaUI.Interfaces;
using GeneralViewModel.Interfaces;

namespace HexViewer.Commands
{
    public static class UserInterfaceCommands
    {
        public static CommandDescriptor OpenFileClick { get; } = new CommandDescriptor("OpenFileClick", typeof(UserInterfaceCommands));
        public static CommandDescriptor ScrollBarClick { get; } = new CommandDescriptor("ScrollBarClick", typeof(UserInterfaceCommands));
    }


    public abstract class NewCommand : CommandBase
    {
        protected object Parameter { get; set; }

        protected IMainViewModel ViewModel { get; set; }

        public NewCommand(IMainViewModel viewModel, object parameter)
        {
            this.ViewModel = viewModel;
            this.Parameter = parameter;
        }

        public override void Cancel()
        {
            //ViewModel.Offset = "Offset";
            //ViewModel.Hex = "Hex";
            //ViewModel.Text = "Text";
            //ViewModel.Value = 0;
        }
    }

    public class OpenFileCommand : NewCommand
    {
        public OpenFileCommand(IMainViewModel viewModel, object parameter) : base(viewModel, parameter)
        {

        }

        public override void Execute()
        {
        }

        public override bool CanExecute()
        {
            return true;
        }
    }

    public class ScrollBarCommand : NewCommand
    {
        public ScrollBarCommand(IMainViewModel viewModel, object parameter) : base(viewModel, parameter)
        {
        }

        public override void Execute()
        {
            if (CanExecute())
            {
                //var Bll = Factory.Get<IBusinesLogicLayerEx>();
            }
        }

        public override bool CanExecute()
        {
            return true;
        }
    }
}
