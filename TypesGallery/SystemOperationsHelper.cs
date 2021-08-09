using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Cryptosoft.TypesGallery
{
	public static class SystemOperationsHelper
	{
		public static void Shutdown()
		{
			var pi = new ProcessStartInfo("shutdown", "/s /t 0")
			{
				UseShellExecute = false,
				CreateNoWindow = true
			};
			Process.Start(pi);
		}		

		public static void Reset()
		{
			var pi = new ProcessStartInfo("shutdown", "/r /t 0")
			{
				UseShellExecute = false,
				CreateNoWindow = true
			};
			Process.Start(pi);
		}

		public static void StartProcess(String procName, bool wait)
		{
			try
			{
				var P = Process.Start(new ProcessStartInfo(procName));
				if (wait)
					P.WaitForExit();
			}
			catch { }

		}

		[DllImport("cspal.qpl")]
		static extern bool LockWorkStation();

		public static void Lock()
		{
			LockWorkStation();
		}
	}
}
