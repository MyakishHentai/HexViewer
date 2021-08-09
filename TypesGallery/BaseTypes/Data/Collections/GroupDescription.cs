using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;

namespace Cryptosoft.TypesGallery.BaseTypes.Data
{
	/// <summary>
	/// Обеспечивает абстрактрный базовый класс для типов, описывающих группировку элементов в коллекции
	/// </summary>
	public abstract class GroupDescription : INotifyPropertyChanged
	{
		private readonly ObservableCollection<object> m_GroupNames = new ObservableCollection<object>();

		/// <summary>
		/// Инициализирует новый экземпляр GroupDescription
		/// </summary>
		protected GroupDescription()
		{
			m_GroupNames.CollectionChanged += OnGroupNamesChanged;
		}

		private void OnGroupNamesChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			OnPropertyChanged(new PropertyChangedEventArgs("GroupNames"));
		}

		/// <summary>
		/// Возвращает коллекцию имен, использующихся при инициализации группы с набором подгрупп
		/// </summary>
		public ObservableCollection<object> GroupNames
		{
			get { return m_GroupNames; }
		}

		/// <summary>
		/// Происходит когда изменяется значения свойств
		/// </summary>
		protected virtual event PropertyChangedEventHandler PropertyChanged;

		event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
		{
			add { PropertyChanged += value; }
			remove { PropertyChanged -= value; }
		}

		/// <summary>
		/// Возвращает имя группы для указанного элемента.
		/// </summary>
		/// <param name="item">Элемент для которого возвращается имя группы.</param>
		/// <param name="level">Уровень группировки.</param>
		/// <param name="culture">CultureInfo для поддержки конвертера.</param>
		/// <returns>Возвращает имя группы для переданного элемента.</returns>
		public abstract object GroupNameFromItem(object item, int level, CultureInfo culture);

		/// <summary>
		/// Возвращает значение, указывающее принадлежит ли элемент группе.
		/// </summary>
		/// <param name="groupName">Имя группы для проверки.</param>
		/// <param name="itemName">Имя элемента для проверки.</param>
		/// <returns>true если элемент принадлежит группе; иначе false.</returns>
		public virtual bool NamesMatch(object groupName, object itemName)
		{
			return Equals(groupName, itemName);
		}

		/// <summary>
		/// Вызывает событие GroupDescription.PropertyChanged
		/// </summary>
		/// <param name="e">Аргументы, с которыми будет вызвано событие</param>
		protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
		{
			var Handler = PropertyChanged;
			if (Handler != null) Handler(this, e);
		}
	}
}