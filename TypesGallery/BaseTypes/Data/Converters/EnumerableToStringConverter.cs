using System;
using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace Cryptosoft.TypesGallery.BaseTypes.Data
{
	public class EnumerableToStringConverter : IValueConverter
	{
		public object Convert(object value, Type targeType, object parameter, CultureInfo culture)
		{
			if (value == null)
				return Binding.DoNothing;

			IEnumerable Value = value as IEnumerable;
			if (Value == null)
				return Binding.DoNothing;

			if (targeType == typeof(string))
			{
				StringBuilder Builder = new StringBuilder();

				foreach (var Item in Value)
				{
					var ParameterAsString = parameter as string;
					if (ParameterAsString != null)
					{
						Type ItemType = Item.GetType();

						PropertyInfo Property = ItemType.GetProperty(ParameterAsString);
						if (Property != null)
							Builder.Append(Property.GetValue(Item));
					}
					else
					{
						Builder.Append(Item);
					}

					Builder.Append(" ");
				}

				return Builder.ToString();
			}

			return Binding.DoNothing;
		}

		public object ConvertBack(object value, Type targeType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}