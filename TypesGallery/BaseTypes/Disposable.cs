using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Cryptosoft.TypesGallery.BaseTypes
{
	internal enum TrySetSingleResult
	{
		Success,
		AlreadyAssigned,
		Disposed
	}

	/// <summary>
	/// Provides a set of static methods for creating <see cref="IDisposable"/> objects.
	/// </summary>
	public static class Disposable
	{
		/// <summary>
		/// Represents a disposable that does nothing on disposal.
		/// </summary>
		private sealed class EmptyDisposable : IDisposable
		{
			/// <summary>
			/// Singleton default disposable.
			/// </summary>
			public static readonly EmptyDisposable Instance = new EmptyDisposable();

			private EmptyDisposable()
			{
			}

			/// <summary>
			/// Does nothing.
			/// </summary>
			public void Dispose()
			{
				// no op
			}
		}

		/// <summary>
		/// Gets the disposable that does nothing when disposed.
		/// </summary>
		public static IDisposable Empty
		{
			get { return EmptyDisposable.Instance; }
		}

		/// <summary>
		/// Creates a disposable object that invokes the specified action when disposed.
		/// </summary>
		/// <param name="dispose">Action to run during the first call to <see cref="IDisposable.Dispose"/>. The action is guaranteed to be run at most once.</param>
		/// <returns>The disposable object that runs the given action upon disposal.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="dispose"/> is <c>null</c>.</exception>
		public static IDisposable Create(Action dispose)
		{
			if (dispose == null)
			{
				throw new ArgumentNullException("dispose");
			}

			return new AnonymousDisposable(dispose);
		}

		/// <summary>
		/// Creates a disposable object that invokes the specified action when disposed.
		/// </summary>
		/// <param name="state">The state to be passed to the action.</param>
		/// <param name="dispose">Action to run during the first call to <see cref="IDisposable.Dispose"/>. The action is guaranteed to be run at most once.</param>
		/// <returns>The disposable object that runs the given action upon disposal.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="dispose"/> is <c>null</c>.</exception>
		public static IDisposable Create<TState>(TState state, Action<TState> dispose)
		{
			if (dispose == null)
			{
				throw new ArgumentNullException("dispose");
			}

			return new AnonymousDisposable<TState>(state, dispose);
		}

		/// <summary>
		/// Gets the value stored in <paramref name="fieldRef" /> or a null if
		/// <paramref name="fieldRef" /> was already disposed.
		/// </summary>
		internal static IDisposable GetValue(ref IDisposable fieldRef)
		{
			var current = Volatile.Read(ref fieldRef);

			return current == BooleanDisposable.True
				? null
				: current;
		}

		/// <summary>
		/// Gets the value stored in <paramref name="fieldRef" /> or a no-op-Disposable if
		/// <paramref name="fieldRef" /> was already disposed.
		/// </summary>
		internal static IDisposable GetValueOrDefault(ref IDisposable fieldRef)
		{
			var current = Volatile.Read(ref fieldRef);

			return current == BooleanDisposable.True
				? EmptyDisposable.Instance
				: current;
		}

		/// <summary>
		/// Assigns <paramref name="value" /> to <paramref name="fieldRef" />.
		/// </summary>
		/// <returns>true if <paramref name="fieldRef" /> was assigned to <paramref name="value" /> and has not
		/// been assigned before.</returns>
		/// <returns>false if <paramref name="fieldRef" /> has been already disposed.</returns>
		/// <exception cref="InvalidOperationException"><paramref name="fieldRef" /> has already been assigned a value.</exception>
		internal static bool SetSingle(ref IDisposable fieldRef, IDisposable value)
		{
			var result = TrySetSingle(ref fieldRef, value);

			if (result == TrySetSingleResult.AlreadyAssigned)
			{
				throw new InvalidOperationException("DISPOSABLE_ALREADY_ASSIGNED");
			}

			return result == TrySetSingleResult.Success;
		}

		/// <summary>
		/// Tries to assign <paramref name="value" /> to <paramref name="fieldRef" />.
		/// </summary>
		/// <returns>A <see cref="TrySetSingleResult"/> value indicating the outcome of the operation.</returns>
		internal static TrySetSingleResult TrySetSingle(ref IDisposable fieldRef, IDisposable value)
		{
			var old = Interlocked.CompareExchange(ref fieldRef, value, null);
			if (old == null)
			{
				return TrySetSingleResult.Success;
			}

			if (old != BooleanDisposable.True)
			{
				return TrySetSingleResult.AlreadyAssigned;
			}

			if (value != null) value.Dispose();
			return TrySetSingleResult.Disposed;
		}

		/// <summary>
		/// Tries to assign <paramref name="value" /> to <paramref name="fieldRef" />. If <paramref name="fieldRef" />
		/// is not disposed and is assigned a different value, it will not be disposed.
		/// </summary>
		/// <returns>true if <paramref name="value" /> was successfully assigned to <paramref name="fieldRef" />.</returns>
		/// <returns>false <paramref name="fieldRef" /> has been disposed.</returns>
		internal static bool TrySetMultiple(ref IDisposable fieldRef, IDisposable value)
		{
			// Let's read the current value atomically (also prevents reordering).
			var old = Volatile.Read(ref fieldRef);

			for (;;)
			{
				// If it is the disposed instance, dispose the value.
				if (old == BooleanDisposable.True)
				{
					if (value != null) value.Dispose();
					return false;
				}

				// Atomically swap in the new value and get back the old.
				var b = Interlocked.CompareExchange(ref fieldRef, value, old);

				// If the old and new are the same, the swap was successful and we can quit
				if (old == b)
				{
					return true;
				}

				// Otherwise, make the old reference the current and retry.
				old = b;
			}
		}

		/// <summary>
		/// Tries to assign <paramref name="value" /> to <paramref name="fieldRef" />. If <paramref name="fieldRef" />
		/// is not disposed and is assigned a different value, it will be disposed.
		/// </summary>
		/// <returns>true if <paramref name="value" /> was successfully assigned to <paramref name="fieldRef" />.</returns>
		/// <returns>false <paramref name="fieldRef" /> has been disposed.</returns>
		internal static bool TrySetSerial(ref IDisposable fieldRef, IDisposable value)
		{
			var copy = Volatile.Read(ref fieldRef);
			for (;;)
			{
				if (copy == BooleanDisposable.True)
				{
					if (value != null) value.Dispose();
					return false;
				}

				var current = Interlocked.CompareExchange(ref fieldRef, value, copy);
				if (current == copy)
				{
					if (copy != null) copy.Dispose();
					return true;
				}

				copy = current;
			}
		}

		/// <summary>
		/// Gets a value indicating whether <paramref name="fieldRef" /> has been disposed.
		/// </summary>
		/// <returns>true if <paramref name="fieldRef" /> has been disposed.</returns>
		/// <returns>false if <paramref name="fieldRef" /> has not been disposed.</returns>
		internal static bool GetIsDisposed(ref IDisposable fieldRef)
		{
			// We use a sentinel value to indicate we've been disposed. This sentinel never leaks
			// to the outside world (see the Disposable property getter), so no-one can ever assign
			// this value to us manually.
			return Volatile.Read(ref fieldRef) == BooleanDisposable.True;
		}

		/// <summary>
		/// Tries to dispose <paramref name="fieldRef" />. 
		/// </summary>
		/// <returns>true if <paramref name="fieldRef" /> was not disposed previously and was successfully disposed.</returns>
		/// <returns>false if <paramref name="fieldRef" /> was disposed previously.</returns>
		internal static bool TryDispose(ref IDisposable fieldRef)
		{
			var old = Interlocked.Exchange(ref fieldRef, BooleanDisposable.True);

			if (old == BooleanDisposable.True)
			{
				return false;
			}

			if (old != null) old.Dispose();
			return true;
		}

		internal static bool TryRelease<TState>(ref IDisposable fieldRef, TState state, Action<IDisposable, TState> disposeAction)
		{
			var old = Interlocked.Exchange(ref fieldRef, BooleanDisposable.True);

			if (old == BooleanDisposable.True)
			{
				return false;
			}

			disposeAction(old, state);
			return true;
		}
	}


	/// <summary>
	/// Represents a disposable resource that can be checked for disposal status.
	/// </summary>
	public sealed class BooleanDisposable : ICancelable
	{
		// Keep internal! This is used as sentinel in other IDisposable implementations to detect disposal and
		// should never be exposed to user code in order to prevent users from swapping in the sentinel. Have
		// a look at the code in e.g. SingleAssignmentDisposable for usage patterns.
		internal static readonly BooleanDisposable True = new BooleanDisposable(true);

		private volatile bool _isDisposed;

		/// <summary>
		/// Initializes a new instance of the <see cref="BooleanDisposable"/> class.
		/// </summary>
		public BooleanDisposable()
		{
		}

		private BooleanDisposable(bool isDisposed)
		{
			_isDisposed = isDisposed;
		}

		/// <summary>
		/// Gets a value that indicates whether the object is disposed.
		/// </summary>
		public bool IsDisposed
		{
			get { return _isDisposed; }
		}

		/// <summary>
		/// Sets the status to disposed, which can be observer through the <see cref="IsDisposed"/> property.
		/// </summary>
		public void Dispose()
		{
			_isDisposed = true;
		}
	}

	/// <summary>
	/// Disposable resource with disposal state tracking.
	/// </summary>
	public interface ICancelable : IDisposable
	{
		/// <summary>
		/// Gets a value that indicates whether the object is disposed.
		/// </summary>
		bool IsDisposed { get; }
	}

	/// <summary>
	/// Represents an Action-based disposable.
	/// </summary>
	internal sealed class AnonymousDisposable : ICancelable
	{
		private volatile Action _dispose;

		/// <summary>
		/// Constructs a new disposable with the given action used for disposal.
		/// </summary>
		/// <param name="dispose">Disposal action which will be run upon calling Dispose.</param>
		public AnonymousDisposable(Action dispose)
		{
			System.Diagnostics.Debug.Assert(dispose != null);

			_dispose = dispose;
		}

		/// <summary>
		/// Gets a value that indicates whether the object is disposed.
		/// </summary>
		public bool IsDisposed
		{
			get { return _dispose == null; }
		}

		/// <summary>
		/// Calls the disposal action if and only if the current instance hasn't been disposed yet.
		/// </summary>
		public void Dispose()
		{
			var Exc = Interlocked.Exchange(ref _dispose, null);
			if (Exc != null) Exc.Invoke();
		}
	}

	/// <summary>
	/// Represents a Action-based disposable that can hold onto some state.
	/// </summary>
	internal sealed class AnonymousDisposable<TState> : ICancelable
	{
		private TState _state;
		private volatile Action<TState> _dispose;

		/// <summary>
		/// Constructs a new disposable with the given action used for disposal.
		/// </summary>
		/// <param name="state">The state to be passed to the disposal action.</param>
		/// <param name="dispose">Disposal action which will be run upon calling Dispose.</param>
		public AnonymousDisposable(TState state, Action<TState> dispose)
		{
			System.Diagnostics.Debug.Assert(dispose != null);

			_state = state;
			_dispose = dispose;
		}

		/// <summary>
		/// Gets a value that indicates whether the object is disposed.
		/// </summary>
		public bool IsDisposed
		{
			get { return _dispose == null; }
		}

		/// <summary>
		/// Calls the disposal action if and only if the current instance hasn't been disposed yet.
		/// </summary>
		public void Dispose()
		{
			var Exc = Interlocked.Exchange(ref _dispose, null);
			if (Exc != null) Exc.Invoke(_state);
			_state = default(TState);
		}
	}

	/// <summary>
	/// Represents a disposable resource that has an associated <seealso cref="CancellationToken"/> that will be set to the cancellation requested state upon disposal.
	/// </summary>
	public sealed class CancellationDisposable : ICancelable
	{
		private readonly CancellationTokenSource _cts;

		/// <summary>
		/// Initializes a new instance of the <see cref="CancellationDisposable"/> class that uses an existing <seealso cref="CancellationTokenSource"/>.
		/// </summary>
		/// <param name="cts"><seealso cref="CancellationTokenSource"/> used for cancellation.</param>
		/// <exception cref="ArgumentNullException"><paramref name="cts"/> is <c>null</c>.</exception>
		public CancellationDisposable(CancellationTokenSource cts)
		{
			if (cts != null)
				_cts = cts;
			else
				throw new ArgumentNullException("cts");
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CancellationDisposable"/> class that uses a new <seealso cref="CancellationTokenSource"/>.
		/// </summary>
		public CancellationDisposable()
			: this(new CancellationTokenSource())
		{
		}

		/// <summary>
		/// Gets the <see cref="CancellationToken"/> used by this <see cref="CancellationDisposable"/>.
		/// </summary>
		public CancellationToken Token
		{
			get { return _cts.Token; }
		}

		/// <summary>
		/// Cancels the underlying <seealso cref="CancellationTokenSource"/>.
		/// </summary>
		public void Dispose()
		{
			_cts.Cancel();
		}

		/// <summary>
		/// Gets a value that indicates whether the object is disposed.
		/// </summary>
		public bool IsDisposed
		{
			get { return _cts.IsCancellationRequested; }
		}
	}

	/// <summary>
	/// Represents a group of disposable resources that are disposed together.
	/// </summary>
	public sealed class CompositeDisposable : ICollection<IDisposable>, ICancelable
	{
		static IDisposable [] s_EmptyArray = new IDisposable[0];
		private readonly object _gate = new object();
		private bool _disposed;
		private List<IDisposable> _disposables;
		private int _count;
		private const int ShrinkThreshold = 64;

		// Default initial capacity of the _disposables list in case
		// The number of items is not known upfront
		private const int DefaultCapacity = 16;

		/// <summary>
		/// Initializes a new instance of the <see cref="CompositeDisposable"/> class with no disposables contained by it initially.
		/// </summary>
		public CompositeDisposable()
		{
			_disposables = new List<IDisposable>();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CompositeDisposable"/> class with the specified number of disposables.
		/// </summary>
		/// <param name="capacity">The number of disposables that the new CompositeDisposable can initially store.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="capacity"/> is less than zero.</exception>
		public CompositeDisposable(int capacity)
		{
			if (capacity < 0)
			{
				throw new ArgumentOutOfRangeException("capacity");
			}

			_disposables = new List<IDisposable>(capacity);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CompositeDisposable"/> class from a group of disposables.
		/// </summary>
		/// <param name="disposables">Disposables that will be disposed together.</param>
		/// <exception cref="ArgumentNullException"><paramref name="disposables"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">Any of the disposables in the <paramref name="disposables"/> collection is <c>null</c>.</exception>
		public CompositeDisposable(params IDisposable[] disposables)
		{
			if (disposables == null)
			{
				throw new ArgumentNullException("disposables");
			}

			Init(disposables, disposables.Length);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CompositeDisposable"/> class from a group of disposables.
		/// </summary>
		/// <param name="disposables">Disposables that will be disposed together.</param>
		/// <exception cref="ArgumentNullException"><paramref name="disposables"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">Any of the disposables in the <paramref name="disposables"/> collection is <c>null</c>.</exception>
		public CompositeDisposable(IEnumerable<IDisposable> disposables)
		{
			if (disposables == null)
			{
				throw new ArgumentNullException("disposables");
			}

			// If the disposables is a collection, get its size
			// and use it as a capacity hint for the copy.

			var c = disposables as ICollection<IDisposable>;

			if (c != null)
			{
				Init(disposables, c.Count);
			}
			else
			{
				// Unknown sized disposables, use the default capacity hint
				Init(disposables, DefaultCapacity);
			}
		}

		/// <summary>
		/// Initialize the inner disposable list and count fields.
		/// </summary>
		/// <param name="disposables">The enumerable sequence of disposables.</param>
		/// <param name="capacityHint">The number of items expected from <paramref name="disposables"/></param>
		private void Init(IEnumerable<IDisposable> disposables, int capacityHint)
		{
			var list = new List<IDisposable>(capacityHint);

			// do the copy and null-check in one step to avoid a
			// second loop for just checking for null items
			foreach (var d in disposables)
			{
				if (d == null)
				{
					throw new ArgumentException("DISPOSABLES_CANT_CONTAIN_NULL", "disposables");
				}
				list.Add(d);
			}

			_disposables = list;
			// _count can be read by other threads and thus should be properly visible
			// also releases the _disposables contents so it becomes thread-safe
			Volatile.Write(ref _count, _disposables.Count);
		}

		/// <summary>
		/// Gets the number of disposables contained in the <see cref="CompositeDisposable"/>.
		/// </summary>
		public int Count
		{
			get { return Volatile.Read(ref _count); }
		}

		/// <summary>
		/// Adds a disposable to the <see cref="CompositeDisposable"/> or disposes the disposable if the <see cref="CompositeDisposable"/> is disposed.
		/// </summary>
		/// <param name="item">Disposable to add.</param>
		/// <exception cref="ArgumentNullException"><paramref name="item"/> is <c>null</c>.</exception>
		public void Add(IDisposable item)
		{
			if (item == null)
			{
				throw new ArgumentNullException("item");
			}

			lock (_gate)
			{
				if (!_disposed)
				{
					_disposables.Add(item);
					// If read atomically outside the lock, it should be written atomically inside
					// the plain read on _count is fine here because manipulation always happens
					// from inside a lock.
					Volatile.Write(ref _count, _count + 1);
					return;
				}
			}

			item.Dispose();
		}

		/// <summary>
		/// Removes and disposes the first occurrence of a disposable from the <see cref="CompositeDisposable"/>.
		/// </summary>
		/// <param name="item">Disposable to remove.</param>
		/// <returns>true if found; false otherwise.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="item"/> is <c>null</c>.</exception>
		public bool Remove(IDisposable item)
		{
			if (item == null)
			{
				throw new ArgumentNullException("item");
			}

			lock (_gate)
			{
				// this composite was already disposed and if the item was in there
				// it has been already removed/disposed
				if (_disposed)
				{
					return false;
				}

				//
				// List<T> doesn't shrink the size of the underlying array but does collapse the array
				// by copying the tail one position to the left of the removal index. We don't need
				// index-based lookup but only ordering for sequential disposal. So, instead of spending
				// cycles on the Array.Copy imposed by Remove, we use a null sentinel value. We also
				// do manual Swiss cheese detection to shrink the list if there's a lot of holes in it.
				//

				// read fields as infrequently as possible
				var current = _disposables;

				var i = current.IndexOf(item);
				if (i < 0)
				{
					// not found, just return
					return false;
				}

				current[i] = null;

				if (current.Capacity > ShrinkThreshold && _count < current.Capacity/2)
				{
					var fresh = new List<IDisposable>(current.Capacity/2);

					foreach (var d in current)
					{
						if (d != null)
						{
							fresh.Add(d);
						}
					}

					_disposables = fresh;
				}

				// make sure the Count property sees an atomic update
				Volatile.Write(ref _count, _count - 1);
			}

			// if we get here, the item was found and removed from the list
			// just dispose it and report success

			item.Dispose();

			return true;
		}

		/// <summary>
		/// Disposes all disposables in the group and removes them from the group.
		/// </summary>
		public void Dispose()
		{
			var currentDisposables = default(List<IDisposable>);
			lock (_gate)
			{
				if (!_disposed)
				{
					currentDisposables = _disposables;
					// nulling out the reference is faster no risk to
					// future Add/Remove because _disposed will be true
					// and thus _disposables won't be touched again.
					_disposables = null;

					Volatile.Write(ref _count, 0);
					Volatile.Write(ref _disposed, true);
				}
			}

			if (currentDisposables != null)
			{
				foreach (var d in currentDisposables)
				{
					if (d != null) d.Dispose();
				}
			}
		}

		/// <summary>
		/// Removes and disposes all disposables from the <see cref="CompositeDisposable"/>, but does not dispose the <see cref="CompositeDisposable"/>.
		/// </summary>
		public void Clear()
		{
			var previousDisposables = default(IDisposable[]);
			lock (_gate)
			{
				// disposed composites are always clear
				if (_disposed)
				{
					return;
				}

				var current = _disposables;

				previousDisposables = current.ToArray();
				current.Clear();

				Volatile.Write(ref _count, 0);
			}

			foreach (var d in previousDisposables)
			{
				if (d != null) d.Dispose();
			}
		}

		/// <summary>
		/// Determines whether the <see cref="CompositeDisposable"/> contains a specific disposable.
		/// </summary>
		/// <param name="item">Disposable to search for.</param>
		/// <returns>true if the disposable was found; otherwise, false.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="item"/> is <c>null</c>.</exception>
		public bool Contains(IDisposable item)
		{
			if (item == null)
			{
				throw new ArgumentNullException("item");
			}

			lock (_gate)
			{
				if (_disposed)
				{
					return false;
				}
				return _disposables.Contains(item);
			}
		}

		/// <summary>
		/// Copies the disposables contained in the <see cref="CompositeDisposable"/> to an array, starting at a particular array index.
		/// </summary>
		/// <param name="array">Array to copy the contained disposables to.</param>
		/// <param name="arrayIndex">Target index at which to copy the first disposable of the group.</param>
		/// <exception cref="ArgumentNullException"><paramref name="array"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="arrayIndex"/> is less than zero. -or - <paramref name="arrayIndex"/> is larger than or equal to the array length.</exception>
		public void CopyTo(IDisposable[] array, int arrayIndex)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}

			if (arrayIndex < 0 || arrayIndex >= array.Length)
			{
				throw new ArgumentOutOfRangeException("arrayIndex");
			}

			lock (_gate)
			{
				// disposed composites are always empty
				if (_disposed)
				{
					return;
				}

				if (arrayIndex + _count > array.Length)
				{
					// there is not enough space beyond arrayIndex 
					// to accommodate all _count disposables in this composite
					throw new ArgumentOutOfRangeException("arrayIndex");
				}
				var i = arrayIndex;
				foreach (var d in _disposables)
				{
					if (d != null)
					{
						array[i++] = d;
					}
				}
			}
		}

		/// <summary>
		/// Always returns false.
		/// </summary>
		public bool IsReadOnly
		{
			get { return false; }
		}

		/// <summary>
		/// Returns an enumerator that iterates through the <see cref="CompositeDisposable"/>.
		/// </summary>
		/// <returns>An enumerator to iterate over the disposables.</returns>
		public IEnumerator<IDisposable> GetEnumerator()
		{
			lock (_gate)
			{
				if (_disposed || _count == 0)
				{
					return EmptyEnumerator;
				}
				// the copy is unavoidable but the creation
				// of an outer IEnumerable is avoidable
				return new CompositeEnumerator(_disposables.ToArray());
			}
		}

		/// <summary>
		/// Returns an enumerator that iterates through the <see cref="CompositeDisposable"/>.
		/// </summary>
		/// <returns>An enumerator to iterate over the disposables.</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		/// <summary>
		/// Gets a value that indicates whether the object is disposed.
		/// </summary>
		public bool IsDisposed
		{
			get { return Volatile.Read(ref _disposed); }
		}

		/// <summary>
		/// An empty enumerator for the <see cref="GetEnumerator"/>
		/// method to avoid allocation on disposed or empty composites.
		/// </summary>
		private static readonly CompositeEnumerator EmptyEnumerator =
			new CompositeEnumerator(s_EmptyArray);

		/// <summary>
		/// An enumerator for an array of disposables.
		/// </summary>
		private sealed class CompositeEnumerator : IEnumerator<IDisposable>
		{
			private readonly IDisposable[] _disposables;
			private int _index;

			public CompositeEnumerator(IDisposable[] disposables)
			{
				_disposables = disposables;
				_index = -1;
			}

			public IDisposable Current
			{
				get { return _disposables[_index]; }
			}

			object IEnumerator.Current
			{
				get { return _disposables[_index]; }
			}

			public void Dispose()
			{
				// Avoid retention of the referenced disposables
				// beyond the lifecycle of the enumerator.
				// Not sure if this happens by default to
				// generic array enumerators though.
				var disposables = _disposables;
				Array.Clear(disposables, 0, disposables.Length);
			}

			public bool MoveNext()
			{
				var disposables = _disposables;

				for (;;)
				{
					var idx = ++_index;
					if (idx >= disposables.Length)
					{
						return false;
					}
					// inlined that filter for null elements
					if (disposables[idx] != null)
					{
						return true;
					}
				}
			}

			public void Reset()
			{
				_index = -1;
			}
		}
	}
}