using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;

namespace Cryptosoft.TypesGallery.Resources
{
	public static class ResourceManager
	{
		const string ApplicationScheme = "application";

		const string Component = "component";

		public static Assembly DefaultAssembly { get; set; }

		public static Uri UriFrom(string path)
		{
			int Index = path.IndexOf("/", StringComparison.Ordinal);

			string AssemblyComponent = Index > 0 ? path.Substring(0, Index) : null;

			if (!string.IsNullOrEmpty(AssemblyComponent) && AssemblyComponent.EndsWith(";" + Component))
			{
				var CallingAssembly = Assembly.GetCallingAssembly();
				return new Uri(CallingAssembly.FullName + ";" + Component + "/" + path, UriKind.Relative);
			}

			return new Uri(path, UriKind.Relative);
		}

		public static Uri UriFrom(Assembly assembly, string path)
		{
			int Index = path.IndexOf("/", StringComparison.Ordinal);

			string AssemblyComponent = Index > 0 ? path.Substring(0, Index) : null;

			if (!string.IsNullOrEmpty(AssemblyComponent) && AssemblyComponent.EndsWith(";" + Component))
				return new Uri(assembly.FullName + ";" + Component + "/" + path, UriKind.Relative);

			throw new ArgumentException();
		}

		public static Stream GetResource(string uri, bool embeddedResource = true)
		{
			return GetResource(new ResourceUri(DefaultAssembly ?? Assembly.GetCallingAssembly(), uri), embeddedResource);
		}

		public static Stream GetResource(Uri uri, bool embeddedResource = true)
		{
			return GetResource(new ResourceUri(DefaultAssembly ?? Assembly.GetCallingAssembly(), uri.OriginalString), embeddedResource);
		}

		public static Stream GetResource(ResourceUri uri, bool embeddedResource = true)
		{
			return GetResourceInternal(uri, embeddedResource);
		}

		static Stream GetResourceInternal(ResourceUri uri, bool embeddedResource)
		{
			// Поддерживаются файлы и ресурсы

			// ""
			if (!uri.IsAbsoluteUri)
			{
				if (!embeddedResource)
					return GetResourceStream(uri.Assembly, uri.OriginalString);

				return GetEmbeddedResourceStream(uri.Assembly, uri.OriginalString) ?? GetResourceStream(uri.Assembly, uri.OriginalString);
			}

			if (Uri.UriSchemeFile.Equals(uri.Scheme))
			{
				throw new NotImplementedException();
			}

			if (ApplicationScheme.Equals(uri.Scheme))
			{
				throw new NotImplementedException();
			}

			throw new NotSupportedException();
		}

		static Stream GetResourceStream(Assembly resourceAssembly, string resourcePath)
		{
			System.Resources.ResourceManager SystemResourceManager = new System.Resources.ResourceManager(
				resourceAssembly.GetName().Name + ".g",
				resourceAssembly);

			using (ResourceSet Set = SystemResourceManager.GetResourceSet(CultureInfo.CurrentCulture, true, true))
			{
				return (UnmanagedMemoryStream)Set.GetObject(resourcePath, true);
			}
		}

		static Stream GetEmbeddedResourceStream(Assembly resourceAssembly, string resourcePath)
		{
			string ResourceName = resourceAssembly.GetName().Name + "." + resourcePath.Replace('/', '.');

			return GetResourceStream(resourceAssembly, resourcePath);
		}

		static readonly Dictionary<UriAndType, object> s_ResourceTable = new Dictionary<UriAndType, object>();

		public static T GetResource<T>(Uri uri, Func<Stream, T> constructor, bool embeddedResource = true)
		{
			return GetResource(new ResourceUri(DefaultAssembly ?? Assembly.GetCallingAssembly(), uri.OriginalString), constructor, embeddedResource);
		}

		public static T GetResource<T>(string uri, Func<Stream, T> constructor, bool embeddedResource = true)
		{
			return GetResource(new ResourceUri(DefaultAssembly ?? Assembly.GetCallingAssembly(), uri), constructor, embeddedResource);
		}

		/// <summary>
		/// Получает ресурс из вызывающей сборки
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="uri"></param>
		/// <param name="constructor"></param>
		/// <param name="embeddedResource"></param>
		/// <returns></returns>
		public static T GetLocalResource<T>(string uri, Func<Stream, T> constructor, bool embeddedResource = true)
		{
			return GetResource(new ResourceUri(Assembly.GetCallingAssembly(), uri), constructor, embeddedResource);
		}

		public static T GetResource<T>(ResourceUri uri, Func<Stream, T> constructor, bool embeddedResource = true)
		{
			UriAndType Key = new UriAndType(uri, typeof(T));

			if (!s_ResourceTable.ContainsKey(Key))
			{
				Stream Str = GetResourceInternal(uri, embeddedResource);

				object Obj = constructor(Str);

				s_ResourceTable[Key] = Obj;
			}

			return (T)s_ResourceTable[Key];
		}

		class UriAndType
		{
			private readonly Uri m_Uri;

			private readonly int m_Hash;
			private readonly Type m_Type;

			public UriAndType(Uri uri, Type type)
			{
				m_Uri = uri;
				m_Type = type;

				m_Hash = MathOperations.GetHashCode(uri, type);
			}

			public override int GetHashCode()
			{
				return m_Hash;
			}

			public override bool Equals(object obj)
			{
				UriAndType Other = obj as UriAndType;

				if (Other == null)
					return false;

				return m_Uri.Equals(Other.m_Uri) && m_Type == Other.m_Type;
			}
		}
	}
}