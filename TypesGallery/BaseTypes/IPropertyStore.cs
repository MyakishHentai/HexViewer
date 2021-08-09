using System;
using System.ComponentModel;

namespace Cryptosoft.TypesGallery.BaseTypes
{
	public interface IPropertyStore
	{
		object GetPoperty(string name);

		void SetProperty(string name, object value);

		event PropertyChangedEventHandler PropertyChanged;

		event PropertyChangedEventHandler PropertyDataChanged;

		IDisposable LockNotifyChanged();

		void UnlockNotifyChanged();
	}
}
