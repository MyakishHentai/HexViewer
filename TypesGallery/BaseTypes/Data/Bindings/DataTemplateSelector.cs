using System;
using System.Collections;
using System.Collections.Generic;

namespace Cryptosoft.TypesGallery.BaseTypes.Data
{
	public interface IDataTemplateSelector
	{
		DataTemplate SelectTemplate(object item, object container);
	}


	public abstract class DataTemplateSelector : IDataTemplateSelector
	{
		public virtual DataTemplate SelectTemplate(object item, object container)
		{
			return null;
		}
	}

	public class TypeTemplateFactory : DataTemplateSelector, IEnumerable
	{
		private Dictionary<Type, DataTemplate> m_Dictionary = new Dictionary<Type, DataTemplate>();

		public void Add(Type conditionType, DataTemplate template)
		{
			m_Dictionary.Add(conditionType, template);
		}

		public IEnumerator GetEnumerator()
		{
			return m_Dictionary.Values.GetEnumerator();
		}

		public override DataTemplate SelectTemplate(object item, object container)
		{
			if (item != null && m_Dictionary.ContainsKey(item.GetType()))
			{
				return m_Dictionary[item.GetType()];
			}

			return base.SelectTemplate(item, container);
		}
	}
}