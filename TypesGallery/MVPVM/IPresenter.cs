using System.Collections.Generic;
using Cryptosoft.TypesGallery.BaseTypes;
using Cryptosoft.TypesGallery.Events;

namespace Cryptosoft.TypesGallery.MVPVM
{
	public interface IPresenter : ITypesFactoryOwner
	{
		void Run(IRoutedEventsNode routedParent);

		void Refresh();

		bool CanStop { get; }

		void Stop();

		IView View { get; }

		IViewModel ViewModel { get; }

		ICollection<IPresenter> Childs { get; }
	}
}
