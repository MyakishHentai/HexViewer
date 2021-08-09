using System.ComponentModel;

namespace Cryptosoft.TypesGallery.BaseTypes
{
	public interface IMetadataPropertyStore
	{
		object GetProperty(string name);

		void SetProperty(string name, object value);

		event PropertyChangedEventHandler PropertyChanged;
	}
}
