using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Cryptosoft.TypesGallery;
using Cryptosoft.TypesGallery.DataCodex;

namespace Cryptosoft.Cryptography
{
	public class Codex<TSystem, TData> : ICodex<TSystem> where TData : ICodexData
	{
		readonly object m_Synchronize = new object();

		private readonly LinkedList<Guideline<TData>> m_TopLevelGuidelines = new LinkedList<Guideline<TData>>();
		private readonly Dictionary<Type, LinkedListNode<Guideline<TData>>> m_TopLevelGuidelinesByName = new Dictionary<Type, LinkedListNode<Guideline<TData>>>();
		private readonly Dictionary<string, GuidelineBase<TData>> m_GuidelinesPool = new Dictionary<string, GuidelineBase<TData>>();
		private readonly Dictionary<string, List<GuidelineBase<TData>>> m_GuidelinesByGroup = new Dictionary<string, List<GuidelineBase<TData>>>(StringComparer.OrdinalIgnoreCase);

		public TData Data { get; set; }

		public void Schedule(Guideline<TData> guideline)
		{
			Type GuidelineType = guideline.GetType();

			if (m_TopLevelGuidelinesByName.ContainsKey(GuidelineType))
				throw new InvalidOperationException();

			m_TopLevelGuidelinesByName[GuidelineType] = m_TopLevelGuidelines.AddLast(guideline);

			guideline.Data = Data;

			UpdateDependencies(guideline);
		}

		private void UpdateDependencies(GuidelineBase<TData> guideline)
		{
			Type GuidelineType = guideline.GetType();

			var Properties = GuidelineType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

			foreach (var Property in Properties)
			{
				var Attr = Property.GetCustomAttribute<ChildGuidelineAttribute>();

				if (Attr == null)
					continue;

				if (!Property.PropertyType.IsSubclassOf(typeof(GuidelineBase<TData>)))
					throw new InvalidCastException();

				var GuidelineAttr = Property.PropertyType.GetCustomAttribute<GuidelineAttribute>();

				string GuidelineName;

				if (GuidelineAttr == null)
					GuidelineName = Property.PropertyType + " " + (Attr.Name ?? Guid.NewGuid().ToString("B"));
				else
					GuidelineName = Property.PropertyType + " " + (Attr.Name ?? GuidelineAttr.Name ?? Guid.NewGuid().ToString("B"));

				GuidelineBase<TData> NewGuideline;

				if (!m_GuidelinesPool.ContainsKey(GuidelineName))
				{
					NewGuideline = (GuidelineBase<TData>)Activator.CreateInstance(Property.PropertyType);

					m_GuidelinesPool[GuidelineName] = NewGuideline;

					NewGuideline.Data = Data;

					UpdateDependencies(NewGuideline);

					foreach (var Group in NewGuideline.Groups)
					{
						if (!m_GuidelinesByGroup.ContainsKey(Group))
							m_GuidelinesByGroup[Group] = new List<GuidelineBase<TData>>();

						m_GuidelinesByGroup[Group].Add(NewGuideline);
					}
				}
				else
				{
					NewGuideline = m_GuidelinesPool[GuidelineName];
				}

				guideline.Dependences.Add(NewGuideline);
				Property.SetValue(guideline, NewGuideline);
			}
		}

		public virtual void Clear()
		{
			foreach (var TopLevelGuideline in m_TopLevelGuidelines)
			{
				TopLevelGuideline.Clear();
			}

			foreach (var Guideline in m_GuidelinesPool.Values)
			{
				Guideline.Clear();
			}
		}

		protected virtual void OnPrepareVerify(TSystem system)
		{ }

		protected virtual void OnVerifySuccess(TSystem system)
		{ }

		public bool Verify(TSystem system)
		{
			lock (m_Synchronize)
			{
				OnPrepareVerify(system);

				Clear();

				bool Result = true;
				foreach (var Item in m_TopLevelGuidelines)
				{
					Result &= Item.Verify();
				}

				if (Result)
					OnVerifySuccess(system);

				Log.Verbose(GetStateDescription());

				return Result;
			}
		}

		public string GetStateDescription()
		{
			StringBuilder Builder = new StringBuilder();

			foreach (var TopLevelGuideline in m_TopLevelGuidelines)
			{
				TopLevelGuideline.MakeDescription(Builder);
			}

			return Builder.ToString();
		}

		public string TraceState()
		{
			StringBuilder Builder = new StringBuilder();

			foreach (var TopLevelGuideline in m_TopLevelGuidelines)
			{
				TopLevelGuideline.TraceState(Builder);
			}

			return Builder.ToString();
		}

		public bool IsComplete(string keyData)
		{
			/*try
			{
				Log.Verbose(m_GuidelinesByGroup[keyData].Select(o => o.Name).ToArray());
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}*/
			return m_GuidelinesByGroup[keyData].All((guideline) => guideline.Complete != false);
		}

		public bool GuidelineComplete(string guidelineName)
		{
			return m_GuidelinesPool[guidelineName].State.Complete ?? false;
		}
	}

	public interface ICodexData
	{
		void Clear();
	}
}
