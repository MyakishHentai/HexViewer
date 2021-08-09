using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Cryptosoft.TypesGallery.Operation
{
	public sealed class Operation : IDisposable, INotifyPropertyChanged
	{
		private readonly List<OperationElement> m_Elements = new List<OperationElement>();
		private OperationElement m_ActiveElement;
		private string m_Title;
		private int m_Length;
		private int m_ActiveElementIndex;
		
		internal LinkedListNode<Operation> ParentNode { get; set; }

		internal OperationStore Parent { get; set; }

		public int Length
		{
			get { return m_Length; }
			private set
			{
				m_Length = value;
				OnPropertyChanged();
			}
		}

		public string Title
		{
			get { return m_Title; }
			set
			{
				m_Title = value;
				OnPropertyChanged();
			}
		}

		public OperationElement ActiveElement
		{
			get { return m_ActiveElement; }
			private set
			{
				m_ActiveElement = value;
				OnPropertyChanged();
			}
		}


		public int ActiveElementIndex
		{
			get { return m_ActiveElementIndex; }
			private set
			{
				m_ActiveElementIndex = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Конструктор создает описание операции с указанным заголовокм
		/// </summary>
		/// <param name="title"></param>
		internal Operation(string title)
		{
			Title = title;
		}

		#region IDisposable Support

		private bool m_Disposed;

		private void Dispose(bool disposing)
		{
			if (m_Disposed) return;

			if (disposing)
			{
				Parent.Stop(this);

				foreach (var Element in m_Elements)
					Element.Dispose();
			}

			m_Disposed = true;
		}

		public void Dispose()
		{
			Dispose(true);
		}

		#endregion

		public OperationElement AddNew(string title = null)
		{
			OperationElement NewProgress = new OperationElement(title);
			NewProgress.Changed += NewProgress_Changed;

			m_Elements.Add(NewProgress);

			Length = m_Elements.Count;

			return NewProgress;
		}

		private void NewProgress_Changed(object sender, EventArgs e)
		{
			OperationElement ChangedElement = (OperationElement)sender;

			if (ActiveElement != ChangedElement)
			{
				ActiveElement = ChangedElement;
				ActiveElementIndex = m_Elements.IndexOf(ActiveElement);
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var Handler = PropertyChanged;
			if (Handler != null) Handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}