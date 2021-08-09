using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cryptosoft.TypesGallery
{	
	public class EventArgs<T> : EventArgs
	{
		private T m_Parameter;

		public EventArgs(T parameter)
		{
			m_Parameter = parameter;
		}

		public T GetParameter()
		{
			return m_Parameter;
		}
	}
}
