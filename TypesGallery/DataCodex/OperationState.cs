using System;
using System.Text;

namespace Cryptosoft.TypesGallery.DataCodex
{
	public class OperationState
	{
		public Exception LastException { get; set; }

		public bool? Complete { get; set; }

		public string Message { get; set; }

		public void Reset()
		{
			LastException = null;
			Complete = null;
			Message = string.Empty;
		}

		public override string ToString()
		{
			StringBuilder Builder = new StringBuilder();

			if(Complete.HasValue)
				Builder.Append(Complete.Value ? "(+)" : "(-)");
			else
				Builder.Append("(?)");

			if (!string.IsNullOrEmpty(Message))
				Builder.Append(" " + Message);

			if (LastException != null)
				Builder.Append("\r\n" + LastException);

			return Builder.ToString();
		}
	}
}