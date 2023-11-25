using System;
using Unity.Collections;

namespace Instancing;

public struct JobInt
{
	private NativeArray<int> Array;

	public int Value
	{
		get
		{
			if (!Array.IsCreated)
			{
				throw new InvalidOperationException("You must call 'JobInt.Create()' before using this in a job");
			}
			return Array[0];
		}
		set
		{
			if (!Array.IsCreated)
			{
				throw new InvalidOperationException("You must call 'JobInt.Create()' before using this in a job");
			}
			Array[0] = value;
		}
	}

	public static JobInt Create()
	{
		JobInt result = default(JobInt);
		result.Array = new NativeArray<int>(1, Allocator.Persistent);
		return result;
	}

	public static void Destroy(JobInt instance)
	{
		NativeArrayEx.SafeDispose(ref instance.Array);
	}
}
