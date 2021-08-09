using System;
using System.Globalization;

namespace Cryptosoft.TypesGallery.BaseTypes.Data
{
	public class PercentToIntegerConverter : IValueConverter
	{
		public object Convert(object value, Type targeType, object parameter, CultureInfo culture)
		{
			if(value == null)
				return Binding.DoNothing;

			if (targeType == typeof(int))
			{
				return (int) ((double) value * (int)parameter);
			}

			return Binding.DoNothing;
		}

		public object ConvertBack(object value, Type targeType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}