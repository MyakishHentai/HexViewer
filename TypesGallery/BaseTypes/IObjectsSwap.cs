using System;

namespace Cryptosoft.TypesGallery.BaseTypes
{
	public interface IObjectsSwap
	{
		T TryGet<T>() where T : class;

		object TryGet(Type type);

		void Push<T>(T item) where T : class;

		void Push(object item);
	}
}