using System;
using System.Collections.Generic;
using System.Text;

namespace Cryptosoft.TypesGallery.MVPVM
{
    public interface IDesktopLifetime
    {
        void Activate(string[] args);

        void SetInstance(IView Window);
    }
}
