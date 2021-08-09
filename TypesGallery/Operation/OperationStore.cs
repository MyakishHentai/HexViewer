using System.Collections.Generic;
using System.ComponentModel;

namespace Cryptosoft.TypesGallery.Operation
{
	/// <summary>
	/// Класс для хранения списка текущих операций
	/// </summary>
	public sealed class OperationStore:INotifyPropertyChanged
	{
		readonly LinkedList<TypesGallery.Operation.Operation> m_List = new LinkedList<TypesGallery.Operation.Operation>();

		/// <summary>
		/// Последняя добавленная операция
		/// </summary>
		public TypesGallery.Operation.Operation Active
		{
			get { return m_List.Last == null ? null : m_List.Last.Value; }
		}

		/// <summary>
		/// Добавление операции
		/// </summary>
		/// <param name="title">Заголовок</param>
		/// <returns>Операция</returns>
		public TypesGallery.Operation.Operation StartNew(string title)
		{
			TypesGallery.Operation.Operation NewOperation = new TypesGallery.Operation.Operation(title);

			lock (m_List)
			{
				NewOperation.ParentNode = m_List.AddLast(NewOperation);
				NewOperation.Parent = this;
			}

			OnPropertyChanged("Active");

			return NewOperation;
		}

		/// <summary>
		/// Завершение операции
		/// </summary>
		/// <param name="operation">Завершаемая операция</param>
		/// <returns>Истина, если операция успешна</returns>
		public bool Stop(TypesGallery.Operation.Operation operation)
		{
			if (operation.ParentNode.List != m_List)
				return false;

			lock (m_List)
			{
				m_List.Remove(operation.ParentNode);
			}

			OnPropertyChanged("Active");

			return true;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged(string propertyName)
		{
			var Handler = PropertyChanged;
			if (Handler != null) Handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}