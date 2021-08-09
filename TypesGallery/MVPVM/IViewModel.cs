using System;
using System.Threading.Tasks;
using Cryptosoft.TypesGallery.BaseTypes;
using Cryptosoft.TypesGallery.Commands;
using Cryptosoft.TypesGallery.Events;

namespace Cryptosoft.TypesGallery.MVPVM
{
	public interface IViewModel : ICommandTarget
	{
		bool CanStop { get; }
		bool Locked { get; set; }

		bool TryStop();

		Task<bool> TryStopAsync();

		IBusinesLogicLayer Content { get; set; }
		
		void Init(IRoutedEventsNode routedParent, ITypesFactory factory);

		void Run();
		void OnStop();
		void Refresh();
	}
}
