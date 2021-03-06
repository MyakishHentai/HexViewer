namespace Cryptosoft.TypesGallery.BaseTypes.Data
{
    /// <summary>
    /// Направление потока данных в привязке, используется в экземплярах класса Binding
    /// </summary>
    /// <remarks>Определяет жесткость и направление связи двух свойств</remarks>
    public enum BindingMode
    {
        /// <summary>
        /// По-умолчанию
        /// </summary>
        /// <remarks>Устанавливаются многоразовые связи от объектов с поддержкой интерфейса INotifyPropertyChanged
        /// Если интерфейс поддерживает один из объектов => Создается многоразовая односторонняя связь от св-ва этого объекта
        /// Если интерфейс не поддерживается обоими объектами => Происходит единоразовое обновление целевого св-ва</remarks>
        Default,
        /// <summary>
        /// Единоразовая передача целевому(вторичному) св-ву значения исходного св-ва
        /// </summary>
        /// <remarks>Обновление св-ва срабатывает при инициализации привязки</remarks>
        OneTime,
        /// <summary>
        /// Единоразовая передача исходному(первичному) св-ву значения целевого св-ва
        /// </summary>
        /// <remarks>Обновление св-ва срабатывает при инициализации привязки</remarks>
        OneTimeToSource,
        /// <summary>
        /// Изменение исходного(первичного) св-ва обновляет целевое
        /// </summary>
        OneWay,
        /// <summary>
        /// Изменение целевого(вторичного) св-ва обновляет исходное
        /// </summary>
        OneWayToSource,
        /// <summary>
        /// Изменение любого из св-в обновляет привязанное
        /// </summary>
        TwoWay
    }
}
