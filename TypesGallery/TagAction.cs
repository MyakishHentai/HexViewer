using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cryptosoft.TypesGallery
{
	public class TagAction
	{
		public Action Action
		{ get; private set; }
		public Object Tag
		{ get; private set; }
		public TagAction(Action action, Object tag = null)
		{
			Action = action;
			Tag = tag;
		}
	}
}
