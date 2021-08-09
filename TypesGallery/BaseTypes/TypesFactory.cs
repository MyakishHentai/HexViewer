using System;
using System.Collections.Generic;

namespace Cryptosoft.TypesGallery.BaseTypes
{
	/// <summary>
	/// Класс фабрики типов
	/// </summary>
	public class TypesFactoryAgregator : ITypesFactory
	{
		private readonly List<TypesFactory> m_ExtendFactories = new List<TypesFactory>();

		private readonly TypesFactory m_FactoryCore = new TypesFactory();

		/// <summary>
		/// Презентатор - владелец фабрики
		/// </summary>
		protected ITypesFactory Parent { get; private set; }

		/// <summary>
		/// Конструктор создает фабрику типов
		/// </summary>
		/// <param name="parent">Презентатор - владелец</param>
		public TypesFactoryAgregator(ITypesFactory parent)
		{
			Parent = parent;
		}

		public TypesFactoryAgregator()
		{
			Parent = null;
		}

		/// <summary>
		/// Регистрация единственного экземпляра объекта
		/// </summary>
		/// <typeparam name="TInterface">Тип интерфейса для которого будет регистрироваться объект</typeparam>
		/// <param name="singleton">Регистрируемый экземпляр</param>
		public void RegisterSingleton<TInterface>(TInterface singleton)
		{
			m_FactoryCore.RegisterSingleton(singleton);
		}

		public void RegisterType<TInterface>(CreateTypeHandler<TInterface> createHandler)
		{
			m_FactoryCore.RegisterType(createHandler);
		}

		public void RegisterType<TInterface, T>() where T : TInterface, new()
		{
			m_FactoryCore.RegisterType<TInterface, T>();
		}

		/// <summary>
		/// Создание экземпляра объекта, реализующего указанный интерфейс
		/// </summary>
		/// <typeparam name="TInterface">Интерфейс, который должен реализовывать создаваемый объект</typeparam>
		/// <returns>Объект, реализующий интерфейс указанный в параметре TInterface</returns>
		public TInterface Get<TInterface>()
		{
			return Get<TInterface>(null);
		}

		/// <summary>
		/// Создание экземпляра объекта, реализующего указанный интерфейс
		/// </summary>
		/// <typeparam name="TInterface">Интерфейс, который должен реализовывать создаваемый объект</typeparam>
		/// <param name="args">Парамеры создания объекта</param>
		/// <returns>Объект, реализующий интерфейс указанный в параметре TInterface</returns>
		public TInterface Get<TInterface>(object args)
		{
			if (m_FactoryCore.IsRegistered<TInterface>())
				return m_FactoryCore.Get<TInterface>(args);

			foreach (TypesFactory ExtendFactory in m_ExtendFactories)
				if (ExtendFactory.IsRegistered<TInterface>())
					return ExtendFactory.Get<TInterface>(args);

			if (Parent!=null)
				return Parent.Get<TInterface>(args);

			throw new InvalidOperationException("Тип " + typeof(TInterface) + " не зарегистрирован в фабрике.");
		}

		public TInterface CreateWithParentPriority<TInterface>()
		{
			return CreateWithParentPriority<TInterface>(null);
		}

		public TInterface CreateWithParentPriority<TInterface>(object args)
		{
			if (Parent != null)
				return Parent.Get<TInterface>(args);

			if (m_FactoryCore.IsRegistered<TInterface>())
				return m_FactoryCore.Get<TInterface>(args);

			foreach (TypesFactory ExtendFactory in m_ExtendFactories)
				if (ExtendFactory.IsRegistered<TInterface>())
					return ExtendFactory.Get<TInterface>(args);

			throw new InvalidOperationException("Тип не найден");
		}

		public void Add(TypesFactory factory)
		{
			if (!m_ExtendFactories.Contains(factory))
				m_ExtendFactories.Add(factory);
		}
	}

	public class TypesFactory
	{
		private readonly Dictionary<Type, Delegate> m_TypesCompliance = new Dictionary<Type, Delegate>();

		/// <summary>
		/// Регистрация единственного экземпляра объекта
		/// </summary>
		/// <typeparam name="TInterface">Тип интерфейса для которого будет регистрироваться объект</typeparam>
		/// <param name="singleton">Регистрируемый экземпляр</param>
		public void RegisterSingleton<TInterface>(TInterface singleton)
		{
			var InterfaceType = typeof(TInterface);

			if (!InterfaceType.IsInterface)
				throw new ArgumentException();

			m_TypesCompliance.Add(InterfaceType, new CreateTypeHandler<TInterface>((args) => singleton));
		}

		public void RegisterSingleton<TInterface, T>() where T : TInterface, new()
		{
			var InterfaceType = typeof(TInterface);
			var ImplementType = typeof(T);

			if (!InterfaceType.IsInterface)
				throw new ArgumentException();

			if (ImplementType.GetInterface(InterfaceType.Name) == null)
				throw new ArgumentException();

			TInterface Singleton = (TInterface)Activator.CreateInstance(ImplementType);

			m_TypesCompliance.Add(InterfaceType, new CreateTypeHandler<TInterface>((args) => Singleton));
		}

		public void RegisterType<TInterface>(CreateTypeHandler<TInterface> createHandler)
		{
			var InterfaceType = typeof(TInterface);

			if (!InterfaceType.IsInterface)
				throw new ArgumentException();

			m_TypesCompliance.Add(InterfaceType, createHandler);
		}

		public void RegisterType<TInterface, T>() where T : TInterface, new()
		{
			var InterfaceType = typeof(TInterface);
			var ImplementType = typeof(T);

			if (!InterfaceType.IsInterface)
				throw new ArgumentException();

			if (ImplementType.GetInterface(InterfaceType.Name) == null)
				throw new ArgumentException();

			m_TypesCompliance.Add(InterfaceType, new CreateTypeHandler<TInterface>((args) => args == null ? (TInterface)Activator.CreateInstance(ImplementType) : (TInterface)Activator.CreateInstance(ImplementType, args)));
		}

		public bool IsRegistered<TInterface>()
		{
			return m_TypesCompliance.ContainsKey(typeof(TInterface));
		}

		/// <summary>
		/// Создание экземпляра объекта, реализующего указанный интерфейс
		/// </summary>
		/// <typeparam name="TInterface">Интерфейс, который должен реализовывать создаваемый объект</typeparam>
		/// <returns>Объект, реализующий интерфейс указанный в параметре TInterface</returns>
		public TInterface Get<TInterface>()
		{
			return Get<TInterface>(null);
		}

		/// <summary>
		/// Создание экземпляра объекта, реализующего указанный интерфейс
		/// </summary>
		/// <typeparam name="TInterface">Интерфейс, который должен реализовывать создаваемый объект</typeparam>
		/// <param name="args">Парамеры создания объекта</param>
		/// <returns>Объект, реализующий интерфейс указанный в параметре TInterface</returns>
		public TInterface Get<TInterface>(object args)
		{
			return (TInterface)m_TypesCompliance[typeof(TInterface)].DynamicInvoke(args);
		}
	}
}