using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cryptosoft.TypesGallery
{
	public static class PrintHelper
	{
		public static String FormatException(this Exception ex)
		{
			StringBuilder Result = new StringBuilder();

			Result.AppendLine(ex.Message);
			Result.AppendFormat("{0}: {1}", "Stack trace", ex.StackTrace);

			if (ex.InnerException != null)
			{
				Result.AppendLine();
				Result.AppendLine();
				Result.AppendLine("Inner exception:");
				Result.Append(FormatException(ex.InnerException));
			}

			if (ex is AggregateException && (ex as AggregateException).InnerExceptions != null)
			{
				foreach (Exception InnerEx in (ex as AggregateException).InnerExceptions)
				{
					Result.AppendLine();
					Result.AppendLine();
					Result.AppendLine("Inner exception:");
					Result.Append(FormatException(InnerEx));
				}
			}

			return Result.ToString();
		}
	}
}
