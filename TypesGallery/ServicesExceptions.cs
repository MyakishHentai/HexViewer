using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cryptosoft.TypesGallery
{
	public class DependentServiceException : ApplicationException
	{
		public DependentServiceException(String message)
			: base(message)
		{ }

		public DependentServiceException(String message, Exception ex)
			: base(message, ex)
		{ }
	}

	public class RequiredServiceException : ApplicationException
	{
		public RequiredServiceException(String message, Exception ex)
			: base(message, ex)
		{ }
	}
}
