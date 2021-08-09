using System;
using System.Reflection;

namespace Cryptosoft.TypesGallery.Resources
{
	public class ResourceUri : Uri
	{
		const string Component = "component";
		private readonly Assembly m_Assembly;

		public Assembly Assembly
		{
			get { return m_Assembly; }
		}

		static string Normalize(string path)
		{
			int Index = path.IndexOf("/", StringComparison.Ordinal);

			string AssemblyComponent = Index > 0 ? path.Substring(0, Index) : null;

			if (!string.IsNullOrEmpty(AssemblyComponent) && AssemblyComponent.EndsWith(";" + Component))
				return Index > 0 ? path.Substring(Index) : null;

			return path;
		}

		static string GetAssembly(string path)
		{
			int Index = path.IndexOf("/", StringComparison.Ordinal);

			string AssemblyComponent = Index > 0 ? path.Substring(0, Index) : null;

			if (!string.IsNullOrEmpty(AssemblyComponent) && AssemblyComponent.EndsWith(";" + Component))
				return AssemblyComponent.Remove(AssemblyComponent.Length - (";" + Component).Length);

			return null;
		}

		public ResourceUri(Assembly assembly, string path) : base(Normalize(path), UriKind.Relative)
		{
			string AssemblyName = GetAssembly(path);

			if (assembly == null)
			{
				m_Assembly = string.IsNullOrEmpty(AssemblyName) ? Assembly.GetCallingAssembly() : Assembly.Load(AssemblyName);
			}
			else
			{
				m_Assembly = string.IsNullOrEmpty(AssemblyName) ? assembly : Assembly.Load(AssemblyName);
			}
		}

		public ResourceUri(string path) : this(null, path)
		{ }

		public override int GetHashCode()
		{
			return MathOperations.GetHashCode(base.GetHashCode(), m_Assembly);
		}

		public override bool Equals(object obj)
		{
			ResourceUri Other = obj as ResourceUri;

			if (Other == null)
				return false;

			return base.Equals(Other) && m_Assembly.Equals(Other.m_Assembly);
		}
	}
}