using System;
using System.Globalization;

namespace Cryptosoft.TypesGallery.BaseTypes
{
    /// <summary>
    /// Интерфейс конвертера значений для механизма привязок
    /// </summary>
    public interface IValueConverter
    {
        /// <summary>
        /// Конвертирует значение из исходного объекта в целевой
        /// </summary>
        /// <param name="value">Конвертируемое значение</param>
        /// <param name="targeType">Тип поля в целевом объекте, к которому требуется привести значение value</param>
        /// <param name="parameter">Дополнительный параметр, использующийся при конвертировании</param>
        /// <param name="culture">Культура, использующаяся при конвертировании</param>
        /// <returns>Конвертированное значение</returns>
        object Convert(object value, Type targeType, object parameter, CultureInfo culture);

        /// <summary>
        /// Конвертирует значение из целевого объекта в исходный
        /// </summary>
        /// <param name="value">Конвертируемое значение</param>
        /// <param name="targeType">Тип поля в исходном объекте, к которому требуется привести значение value</param>
        /// <param name="parameter">Дополнительный параметр, использующийся при конвертировании</param>
        /// <param name="culture">Культура, использующаяся при конвертировании</param>
        /// <returns>Конвертированное значение</returns>
        object ConvertBack(object value, Type targeType, object parameter, CultureInfo culture);
    }
}
