using System;

namespace Cryptosoft.TypesGallery.BaseTypes
{
	public delegate TInterface CreateTypeHandler<out TInterface>(object args);

	public interface ITypesFactoryOwner
	{
		ITypesFactoryOwner Parent { get; }

		ITypesFactory Factory { get; }
	}

	public interface ITypesFactory
	{
		TInterface Get<TInterface>();

		TInterface Get<TInterface>(object args);
	}
}
