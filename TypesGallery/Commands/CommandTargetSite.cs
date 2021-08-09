using System;

namespace Cryptosoft.TypesGallery.Commands
{
	public class CommandTargetSite
	{
		public event EventHandler Changed;

		public void Change()
		{
			if (Changed != null)
				Changed(this, EventArgs.Empty);
		}
	}
}
