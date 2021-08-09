using Cryptosoft.TypesGallery.MVPVM;
using System;
using System.Windows.Forms;

namespace View.WinForms
{
    public class BuildApp : IDesktopLifetime
    {
        public void Activate(string[] args)
        {
            throw new NotImplementedException();
        }

        public void SetInstance(IView Window)
        {
            throw new NotImplementedException();
        }

        public static void TextRendering()
        {
            Application.SetCompatibleTextRenderingDefault(false);
        }
    }
}
