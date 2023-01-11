using System;

public class SimpleList<T>
{
	private const int defaultCapacity = 16;

	private static readonly T[] emptyArray = new T[0];

	public T[] array;

	public int count;

	public T[] Array => array;

	public int Count => count;

	public int Capacity
	{
		get
		{
			return array.Length;
		}
		set
		{
			if (value == array.Length)
			{
				return;
			}
			if (value > 0)
			{
				T[] destinationArray = new T[value];
				if (count > 0)
				{
					System.Array.Copy(array, 0, destinationArray, 0, count);
				}
				array = destinationArray;
			}
			else
			{
				array = emptyArray;
			}
		}
	}

	public T this[int index]
	{
		get
		{
			return array[index];
		}
		set
		{
			array[index] = value;
		}
	}

	public SimpleList()
	{
		array = emptyArray;
	}

	public SimpleList(int capacity)
	{
		array = ((capacity == 0) ? emptyArray : new T[capacity]);
	}

	public void Add(T item)
	{
		if (count == array.Length)
		{
			EnsureCapacity(count + 1);
		}
		array[count++] = item;
	}

	public void Clear()
	{
		if (count > 0)
		{
			System.Array.Clear(array, 0, count);
			count = 0;
		}
	}

	public bool Contains(T item)
	{
		for (int i = 0; i < count; i++)
		{
			if (array[i].Equals(item))
			{
				return true;
			}
		}
		return false;
	}

	public void CopyTo(T[] array)
	{
		System.Array.Copy(this.array, 0, array, 0, count);
	}

	public void EnsureCapacity(int min)
	{
		if (array.Length < min)
		{
			int num = ((array.Length == 0) ? 16 : (array.Length * 2));
			num = ((num < min) ? min : num);
			Capacity = num;
		}
	}
}
