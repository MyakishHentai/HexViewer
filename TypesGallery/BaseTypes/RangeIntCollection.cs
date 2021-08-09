using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Cryptosoft.TypesGallery.BaseTypes
{
	[Serializable]
	public class RangeIntCollection : ICollection<int>, ISerializable
	{
		[Serializable]
		sealed class Range : IEquatable<Range>, ISerializable
		{
			public int Start { get; internal set; }
			public int End { get; internal set; }

			public bool IsSingle { get { return Start == End; } }

			public int Count { get { return End - Start + 1; } }

			public Range(int start, int end)
			{
				if (start > end)
					throw new ArgumentException();

				Start = start;
				End = end;
			}

			public Range(int startAndEnd)
			{
				Start = startAndEnd;
				End = startAndEnd;
			}

			private Range(SerializationInfo info, StreamingContext context)
			{
				Start = (int)info.GetValue("Start", typeof(int));
				End = (int)info.GetValue("End", typeof(int));
			}

			public int this[int index]
			{
				get
				{
					if (index < 0 || index >= Count)
						throw new IndexOutOfRangeException();

					return Start + index;
				}
			}

			public override bool Equals(object obj)
			{
				return Equals(obj as Range);
			}

			public bool Equals(Range other)
			{
				return other != null &&
					   Start == other.Start &&
					   End == other.End;
			}

			public override int GetHashCode()
			{
				var hashCode = -1676728671;
				hashCode = hashCode * -1521134295 + Start.GetHashCode();
				hashCode = hashCode * -1521134295 + End.GetHashCode();
				return hashCode;
			}

			public static bool operator ==(Range range1, Range range2)
			{
				return EqualityComparer<Range>.Default.Equals(range1, range2);
			}

			public static bool operator !=(Range range1, Range range2)
			{
				return !(range1 == range2);
			}

			public bool Contains(int item)
			{
				return item >= Start && item <= End;
			}

			public static bool CanUnion(Range range1, Range range2)
			{
				return (range1.Contains(range2.Start) || range1.Contains(range2.End) || range2.Contains(range1.Start) || range2.Contains(range1.End) ||
						range1.End == range2.Start - 1 || range2.End == range1.Start - 1);
			}

			public static Range Union(Range range1, Range range2)
			{
				if (!CanUnion(range1, range2))
					throw new ArgumentException();

				return new Range(Math.Min(range1.Start, range2.Start), Math.Max(range1.End, range2.End));
			}

			public void GetObjectData(SerializationInfo info, StreamingContext context)
			{
				info.AddValue("Start", Start);
				info.AddValue("End", End);
			}
		}

		class RangeEnumerator : IEnumerator<int>
		{
			RangeIntCollection m_Collection;

			int m_Index = -1;
			LinkedListNode<Range> m_CurrentNode;

			public int Current { get; private set; }

			object IEnumerator.Current { get { return Current; } }

			public RangeEnumerator(RangeIntCollection collection)
			{
				m_Collection = collection;
				m_CurrentNode = m_Collection.m_Ranges.First;
			}

			public void Dispose() { }

			public bool MoveNext()
			{
				while (m_CurrentNode != null)
				{
					if (++m_Index >= m_CurrentNode.Value.Count)
					{
						m_CurrentNode = m_CurrentNode.Next;
						m_Index = -1;
					}
					else
					{
						break;
					}
				}

				if (m_CurrentNode == null)
					return false;

				Current = m_CurrentNode.Value[m_Index];

				return true;
			}

			public void Reset()
			{
				m_CurrentNode = m_Collection.m_Ranges.First;
				m_Index = -1;
			}
		}

		LinkedList<Range> m_Ranges = new LinkedList<Range>();

		public int Count { get; private set; }

		public bool IsReadOnly { get { return false; } }

		public RangeIntCollection()
		{
		}

		public RangeIntCollection(int start, int length)
		{
			Count = length;
			m_Ranges.AddFirst(new Range(start, start + length - 1));
		}

		protected RangeIntCollection(SerializationInfo info, StreamingContext context)
		{
			Count = (int)info.GetValue("Count", typeof(int));
			m_Ranges = (LinkedList<Range>)info.GetValue("Ranges", typeof(LinkedList<Range>));
		}

		public void Add(int item)
		{
			LinkedListNode<Range> NextNode = m_Ranges.First;

			if (NextNode == null)
			{
				m_Ranges.AddFirst(new Range(item));
				Count++;
				return;
			}

			while (NextNode != null)
			{
				if (item < NextNode.Value.Start - 1)
				{
					// Добавляем новый диапазон
					m_Ranges.AddBefore(NextNode, new Range(item));
					Count++;
					return;
				}
				else if (item == NextNode.Value.Start - 1)
				{
					// расширяем диапазон
					NextNode.Value.Start--;
					Count++;
					return;
				}
				else if (item <= NextNode.Value.End)
				{
					return;
				}
				else if (item == NextNode.Value.End + 1)
				{
					// расширяем диапазон
					NextNode.Value.End++;
					Count++;

					if (NextNode.Next != null && Range.CanUnion(NextNode.Value, NextNode.Next.Value))
					{
						NextNode.Value = Range.Union(NextNode.Value, NextNode.Next.Value);

						m_Ranges.Remove(NextNode.Next);
					}

					return;
				}
				else
				{
					NextNode = NextNode.Next;
				}
			}

			m_Ranges.AddLast(new Range(item));
			Count++;
		}

		public void Clear()
		{
			Count = 0;
			m_Ranges.Clear();
		}

		public bool Contains(int item)
		{
			LinkedListNode<Range> NextNode = m_Ranges.First;

			while (NextNode != null)
			{
				if (item < NextNode.Value.Start)
					return false;

				if (item <= NextNode.Value.End)
					return true;

				NextNode = NextNode.Next;
			}

			return false;
		}

		public void CopyTo(int[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		public int this[int index]
		{
			get
			{
				if (index < 0)
					throw new IndexOutOfRangeException();

				if (index >= Count)
					throw new IndexOutOfRangeException();

				LinkedListNode<Range> NextNode = m_Ranges.First;

				while (NextNode != null)
				{
					if (index > NextNode.Value.Count)
						index -= NextNode.Value.Count;
					else
						return NextNode.Value.Start + index;

					NextNode = NextNode.Next;
				}

				throw new IndexOutOfRangeException();
			}
		}

		public IEnumerator<int> GetEnumerator()
		{
			return new RangeEnumerator(this);
		}

		public bool Remove(int item)
		{
			LinkedListNode<Range> NextNode = m_Ranges.First;

			while (NextNode != null)
			{
				if (item < NextNode.Value.Start)
				{
					return false;
				}
				else if (item == NextNode.Value.Start)
				{
					if (NextNode.Value.IsSingle)
						m_Ranges.Remove(NextNode);
					else
						NextNode.Value.Start++;
					Count--;
					return true;
				}
				else if (item < NextNode.Value.End)
				{
					m_Ranges.AddBefore(NextNode, new Range(NextNode.Value.Start, item - 1));
					NextNode.Value.Start = item + 1;
					Count--;
					break;
				}
				else if (item == NextNode.Value.End)
				{
					if (NextNode.Value.IsSingle)
						m_Ranges.Remove(NextNode);
					else
						NextNode.Value.End--;
					Count--;
					return true;
				}
				else
				{
					NextNode = NextNode.Next;
				}
			}

			return false;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("Count", Count);
			info.AddValue("Ranges", m_Ranges, typeof(LinkedList<Range>));
		}
	}
}