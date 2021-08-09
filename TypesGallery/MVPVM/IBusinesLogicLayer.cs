using Cryptosoft.TypesGallery.BaseTypes;
using Cryptosoft.TypesGallery.Commands;

namespace Cryptosoft.TypesGallery.MVPVM
{
	public interface IBusinesLogicLayer : ICommandTarget
	{
		bool IsChanged { get; }
		ITypesFactory Factory { get; }
	}
}
