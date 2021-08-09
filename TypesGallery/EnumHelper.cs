using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Cryptosoft.TypesGallery
{
	[AttributeUsage(AttributeTargets.Field)]
	public sealed class DescriptionAttribute : Attribute
	{
		public String Value
		{ get; private set; }

		public DescriptionAttribute(String value)
		{
			Value = value;
		}		
	}

	public static class EnumHelper
	{
		public static String Description(this Enum enumItem)
		{			
			if (null != enumItem.GetType().GetCustomAttribute<FlagsAttribute>())
			{
				// Если enum имеет атрибут Flags, тут нужно поднапрячься...

				var ToStrings = enumItem.ToString().Split(',');
				var Descs = new List<String>();

				foreach (var Str in ToStrings)
				{
					DescriptionAttribute StatusDescr =
						enumItem.GetType().GetField(Str.Trim()).GetCustomAttribute<DescriptionAttribute>();

					if (StatusDescr != null)
						Descs.Add(StatusDescr.Value);
					else
						Descs.Add(Str);
				}

				return Descs.PrintSequence(", ", "");
			}
			else
			{
				DescriptionAttribute StatusDescr =
				enumItem.GetType().GetField(enumItem.ToString()).GetCustomAttribute<DescriptionAttribute>();

				if (StatusDescr != null)
					return StatusDescr.Value;
				else
					return enumItem.ToString();
			}			
		}
	}	
}
