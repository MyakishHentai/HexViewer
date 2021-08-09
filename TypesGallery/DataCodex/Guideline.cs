using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Cryptosoft.Cryptography;

namespace Cryptosoft.TypesGallery.DataCodex
{
	public abstract class Guideline<TData, TArg1, TArg2> : GuidelineBase<TData>
	{
		protected abstract bool OnVerify(TArg1 arg1, TArg2 arg2);

		public bool Verify(TArg1 arg1, TArg2 arg2) { return VerifyAndUpdateState(() => OnVerify(arg1, arg2)); }
	}

	public abstract class Guideline<TData, TArg> : GuidelineBase<TData>
	{
		protected abstract bool OnVerify(TArg arg);

		public bool Verify(TArg arg) { return VerifyAndUpdateState(() => OnVerify(arg)); }
	}

	public abstract class Guideline<TData> : GuidelineBase<TData>
	{
		protected abstract bool OnVerify();

		public bool Verify() { return VerifyAndUpdateState(OnVerify); }
	}

	[GuidelineDescription("Имя не определено", Description = "Описание не определено")]
	public abstract class GuidelineBase<TData>
	{
		private bool m_Validated;
		private readonly OperationState m_State = new OperationState();
		private readonly List<GuidelineBase<TData>> m_Dependences = new List<GuidelineBase<TData>>();

		private string m_Name;
		private string m_Description;

		public string Name
		{
			get
			{
				if (string.IsNullOrEmpty(m_Name))
				{
					Type CurrentType = GetType();

					var Attr = CurrentType.GetCustomAttribute<GuidelineDescriptionAttribute>();

					if (Attr != null)
						m_Name = Attr.Name;

					if (string.IsNullOrEmpty(m_Name))
						m_Name = CurrentType.Name;
				}

				return m_Name;
			}
		}

		public string Description
		{
			get
			{
				if (string.IsNullOrEmpty(m_Description))
				{
					Type CurrentType = GetType();

					var Attr = CurrentType.GetCustomAttribute<GuidelineDescriptionAttribute>();

					if (Attr != null)
						m_Description = Attr.Description;

					if (string.IsNullOrEmpty(m_Description))
						m_Description = string.Empty;
				}

				return m_Description;
			}
		}

		public List<GuidelineBase<TData>> Dependences { get { return m_Dependences; } }

		protected virtual IEnumerable<string> GetGroupNames() { return Enumerable.Empty<string>(); }

		public bool? Complete
		{
			get { return m_State.Complete; }
		}

		public OperationState State
		{
			get { return m_State; }
		}

		public TData Data { get; internal set; }
		public IEnumerable<string> Groups { get { return GetGroupNames(); } }

		public bool VerifyAndUpdateState(Func<bool> verify)
		{
			State.Reset();

			try
			{
				State.Complete = verify();
			}
			catch (Exception Ex)
			{
				State.Complete = false;
				State.LastException = Ex;
				Log.Verbose("Произошла ошибка в " + Name, Ex);
			}

			return Complete ?? false;
		}
		public override string ToString()
		{
			return Name.ToString();
		}

		public void Invalidate()
		{
			m_Validated = false;
		}

		public void Clear()
		{
			State.Reset();
		}


		public void MakeDescription(StringBuilder builder)
		{
			MakeDescriptionCore(builder, 0);
		}

		public void TraceState(StringBuilder builder)
		{
			TraceStateCore(builder, 0);
		}

		protected virtual void TraceStateCore(StringBuilder builder, int level)
		{
			string Tabs = new string(' ', level * 2);
			builder.AppendLine(Tabs + Name + " " + State);

			foreach (var Depend in Dependences)
			{
				Depend.TraceStateCore(builder, level + 1);
			}
		}


		protected virtual void MakeDescriptionCore(StringBuilder builder, int level)
		{
			if (State.Complete != true)
			{
				string Tabs = new string(' ', level * 2);
				builder.AppendLine(Tabs + Name + " " + State);

				foreach (var Depend in Dependences)
				{
					Depend.MakeDescriptionCore(builder, level + 1);
				}
			}
		}
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class GuidelineDescriptionAttribute : Attribute
	{
		public string Name { get; private set; }

		public string Description { get; set; }

		public GuidelineDescriptionAttribute(string name)
		{
			Name = name;
		}
	}

	[AttributeUsage(AttributeTargets.Property)]
	public class ChildGuidelineAttribute : Attribute
	{
		public string Name { get; set; }
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class GuidelineAttribute : Attribute
	{
		public string Name { get; set; }
	}

}