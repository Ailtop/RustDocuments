namespace Unity.Collections;

public static class NativeArrayEx
{
	public static void Add<T>(this ref NativeArray<T> array, T item, ref int size) where T : unmanaged
	{
		if (size >= array.Length)
		{
			Expand(ref array, array.Length * 2);
		}
		array[size] = item;
		size++;
	}

	public static void RemoveUnordered<T>(this ref NativeArray<T> array, int index, ref int count) where T : unmanaged
	{
		int num = count - 1;
		if (index != num)
		{
			array[index] = array[num];
		}
		count--;
	}

	public static void Expand<T>(this ref NativeArray<T> array, int newCapacity) where T : unmanaged
	{
		if (newCapacity > array.Length)
		{
			NativeArray<T> nativeArray = new NativeArray<T>(newCapacity, Allocator.Persistent);
			if (array.IsCreated)
			{
				array.CopyTo(nativeArray.GetSubArray(0, array.Length));
				array.Dispose();
			}
			array = nativeArray;
		}
	}

	public static void SafeDispose<T>(this ref NativeArray<T> array) where T : unmanaged
	{
		if (array.IsCreated)
		{
			array.Dispose();
		}
	}
}
