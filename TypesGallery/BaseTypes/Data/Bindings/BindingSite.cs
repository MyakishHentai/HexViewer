using System.Collections.Generic;

namespace Cryptosoft.TypesGallery.BaseTypes.Data
{
	public class BindingSite
	{
		private readonly List<Binding> m_Bindings = new List<Binding>();

		public IBindingTarget Target { get; private set; }

		public BindingSite(IBindingTarget target)
		{
			Target = target;
		}

		public void Clear()
		{
			foreach (var Binding in m_Bindings)
			{
				Binding.Clear();
			}

			m_Bindings.Clear();
		}

		public void Update(DataTemplate template)
		{
			if (template == null) return;

			Clear();

			foreach (var Binding in template.Bindings)
			{
				m_Bindings.Add(Binding.Clone(Binding.Target ?? Target, Binding.Target == null, Binding.Source ?? Target.DataSource, Binding.Source == null));
			}
		}

		public void InvalidateTarget()
		{
			foreach (var Binding in m_Bindings)
			{
				Binding.UpdateTarget();
			}
		}

        public void Add(Binding binding)
		{
			m_Bindings.Add(binding);

			binding.UpdateTarget();
		}

		public Binding Add(string targetPath, string sourcePath, BindingMode mode = BindingMode.Default)
		{
			var Bind = new Binding(Target, targetPath, Target.DataSource, sourcePath, mode);

			m_Bindings.Add(Bind);

			if (mode != BindingMode.OneTimeToSource && mode != BindingMode.OneWayToSource)
				Bind.UpdateTarget();

			return Bind;
		}

		public void Invalidate()
		{
			foreach (var Binding in m_Bindings)
			{
				if (Binding.BindToDataSource)
					Binding.Source = Target.DataSource;
			}
		}
	}

	public interface IBindingTarget
	{
		object DataSource { get; set; }
	}
}
