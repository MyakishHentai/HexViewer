using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Cryptosoft.TypesGallery
{
	public static class IpAddressHelper
	{
		public static bool IsZero(this IPAddress value)
		{
			var Bytes = value.GetAddressBytes();
			foreach (Byte B in Bytes)
			{
				if (B != 0)
				{
					return false;
				}
			}
			return true;
		}

		public static bool IsMask(this IPAddress value)
		{
			uint Addr = (uint)value.Address;
			Boolean ThereWereZeros = false;

			if (Addr == 0)
				return false;

			// МАСКА. Сначала должны идти единички, потом нули и только нули

			for (int i = 0; i < 32; i++)
			{
				UInt32 Bit = Addr & 1;

				Addr = Addr >> 1;

				if (Bit == 1)
				{
					if (ThereWereZeros)
						return false;
				}
				else
				{
					ThereWereZeros = true;
				}

			}

			return true;
		}
	}
}
