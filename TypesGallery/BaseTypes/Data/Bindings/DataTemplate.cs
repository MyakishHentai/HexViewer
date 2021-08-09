using System;
using System.Collections.Generic;

namespace Cryptosoft.TypesGallery.BaseTypes.Data
{
	public class DataTemplate
	{
		private readonly List<Binding> m_Bindings = new List<Binding>();

		public List<Binding> Bindings
		{
			get { return m_Bindings; }
		}
	}

	public class DataControlTemplate : DataTemplate
	{
		public Type ControlType { get; private set; }

		public Func<object, object, IObjectsSwap, object> ControlConstructor { get; private set; }

		public DataControlTemplate(Type controlType, Func<object, object, IObjectsSwap, object> controlConstructor)
		{
			ControlType = controlType;
			ControlConstructor = controlConstructor;
		}
	}

}