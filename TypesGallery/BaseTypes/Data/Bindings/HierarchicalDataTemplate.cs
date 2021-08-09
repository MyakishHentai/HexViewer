using System.Collections;

namespace Cryptosoft.TypesGallery.BaseTypes.Data
{
	public class HierarchicalDataTemplate : DataTemplate
	{
		public DataTemplate ItemTemplate { get; set; }

		public DataTemplateSelector ItemTemplateSelector { get; set; }

		public Binding ItemSource { get; set; }

		public IEnumerable Items { get; set; }
	}
}