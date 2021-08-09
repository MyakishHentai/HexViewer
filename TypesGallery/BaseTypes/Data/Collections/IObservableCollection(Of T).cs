using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Cryptosoft.TypesGallery.BaseTypes.Data
{
	public interface IObservableCollection<T> : ICollection, ICollection<T>, INotifyCollectionChanged, INotifyPropertyChanged		
	{ }
}
