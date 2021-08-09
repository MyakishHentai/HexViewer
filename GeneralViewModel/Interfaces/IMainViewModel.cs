using Cryptosoft.TypesGallery.MVPVM;
using System;

namespace GeneralViewModel.Interfaces
{
    public interface IMainViewModel : IViewModel
    {
        string FilePath { get; set; }

        double Value { get; set; }

        long MaxLines { get; set; }

        string Offset { get; set; }

        string Hex { get; set; }

        string Text { get; set; }

        int VisibleLines { get; set; }

        void ChangeScrolls();

        void UpdateFileDisplay(double value);
    }
}
