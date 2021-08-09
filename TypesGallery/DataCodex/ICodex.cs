using System;

namespace Cryptosoft.TypesGallery.DataCodex
{
	public interface ICodex<TSystem>
	{
		bool Verify(TSystem system);
		void Clear();
		string GetStateDescription();
		string TraceState();
	}
}