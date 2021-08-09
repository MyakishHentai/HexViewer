using System;
using System.Linq;
using System.Threading.Tasks;
using Cryptosoft.TypesGallery.Events;

namespace Cryptosoft.TypesGallery
{
	public class UiMessageEventArgs : RoutedEventArgs
	{
		private object m_Result;
		private string m_Caption = "Администратор криптографической подсистемы";
		private Task<Choice> m_ResultTask;
		private ChoiceCollection m_Choices;

		/// <summary>
		/// Текст общей оповещающей ифнормации
		/// </summary>
		public string Message { get; set; }

		/// <summary>
		/// Информация, выводимая в отдельной вкладке оповещения
		/// </summary>
		public object ExtendedData { get; set; }

		/// <summary>
		/// Заголовок
		/// </summary>
		public string Caption
		{
			get { return m_Caption; }
			set { m_Caption = value; }
		}

		public object Result
		{
			get { return m_Result; }
			//set { m_Result = value; }
		}

		public void SelectResult(Choice choice)
		{
			if (!Choices.Contains(choice))
				throw new InvalidOperationException();

			m_Result = choice.Value;
		}

		public void SelectResultAsync(Task<Choice> choice)
		{
			m_ResultTask = choice;
		}

		public ChoiceCollection Choices
		{
			get { return m_Choices == null ? ChoiceCollection.Ok : m_Choices; }
			set { m_Choices = value; }
		}

		public bool IsAsync
		{
			get { return m_ResultTask != null; }
		}

		public bool Returns(Choice choice)
		{
			return Result == choice.Value;
		}

		public async Task WaitForReturnAsync()
		{
			if (!IsAsync)
				return;

			var Choice = await m_ResultTask;

			SelectResult(Choice);
		}
	}
}