using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cryptosoft.TypesGallery
{

	/// <summary>
	/// Класс описывает логику работы кнопок (или чегото похожего) "Вперёд" и "Назад" или "Undo" и "Redo".
	/// При совершении действия следует вызвать NewStep, передав туда "шаг".
	/// Для переходов назад и вперёд вызываются GoPrev и GoForward, методы вернут нужные "шаги".
	/// Свойства HasPrev и HasForward позволяют узнать, есть ли шаги сзади и спереди.
	/// </summary>
	public class StepsManager<T>
	{
		List<T> m_Stack = new List<T>();
		int m_Position = 0;

		public StepsManager()
		{ }

		public bool HasBack
		{
			get { return m_Position > 0; }
		}

		public bool HasForward
		{
			get { return m_Position < m_Stack.Count - 1; }
		}

		public T GoBack()
		{
			if (!HasBack)
				throw new InvalidOperationException();

			m_Position--;
			return m_Stack[m_Position];
		}

		public T GoForward()
		{
			if (!HasForward)
				throw new InvalidOperationException();

			m_Position++;
			return m_Stack[m_Position];
		}

		public void NewStep(T newStep)
		{
			if (HasForward)
			{
				do
					m_Stack.RemoveAt(m_Stack.Count - 1);
				while (HasForward);
			}

			m_Stack.Add(newStep);
			m_Position = m_Stack.Count - 1;
		}

		public void RemoveAll(Predicate<T> removeCond)
		{
			m_Stack.RemoveAll(removeCond);
			m_Position = m_Stack.Count - 1;
		}
	}
}
