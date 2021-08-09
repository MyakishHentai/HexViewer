using System;
using System.ComponentModel;
using Cryptosoft.TypesGallery.BaseTypes.Data;

namespace Cryptosoft.TypesGallery.MVPVM
{
	/// <summary>
	/// Базовый интерфейс представления
	/// </summary>
	public interface IView : IBindingTarget, IDisposable
	{
		event EventHandler<ViewClosedEventArgs> ViewClosed;

		/// <summary>
		/// Функция вызывается при каждом отображении представления
		/// </summary>
		/// <param name="parentView">Родительское представление</param>
		void Activate(IView parentView);

		/// <summary>
		/// При реализации функция должна закрывать представление
		/// </summary>
		void Close();

		/// <summary>
		/// Функция вызывается при скрытии представления
		/// </summary>
		void OnHide();
	}

	public interface IView<TViewModel> : IView
		where TViewModel : class, IViewModel
	{
		TViewModel Model { get; set; }
	}

	public class ViewClosedEventArgs : EventArgs
	{
	}
}
