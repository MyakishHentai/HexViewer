using System;

namespace Cryptosoft.TypesGallery.BaseTypes.Data
{
	/// <summary>
	/// Представляет метод, который обрабатывает событие ICollectionView.CurrentChanging.
	/// </summary>
	/// <param name="sender">Источник события.</param>
	/// <param name="args">Информация о событии.</param>
	public delegate void CurrentChangingEventHandler(object sender, CurrentChangingEventArgs args);

	/// <summary>
	/// Содержит информацию для события ICollectionView.CurrentChanging
	/// </summary>
	public class CurrentChangingEventArgs : EventArgs
	{
		/// <summary>
		/// Инициализирует новый экземпляр CurrentChangingEventArgs.
		/// </summary>
		public CurrentChangingEventArgs() : this(false) { }

		/// <summary>
		/// Инициализирует новый экземпляр CurrentChangingEventArgs с указанным значением поля IsCancelable.
		/// </summary>
		/// <param name="isCancelable">Значение, указавающее является ли событие отменяемым</param>
		public CurrentChangingEventArgs(bool isCancelable)
		{
			IsCancelable = isCancelable;
		}

		/// <summary>
		/// Возвращает или устанавливает значение, указывающее отменено ли событие.
		/// </summary>
		/// <exception cref="InvalidOperationException">Если свойство CurrentChangingEventArgs.IsCancelable имеет значение false.</exception>
		public bool Cancel { get; set; }

		/// <summary>
		/// Возвращет значение, является ли событие отменяемым.
		/// </summary>
		public bool IsCancelable { get; private set; }
	}
}