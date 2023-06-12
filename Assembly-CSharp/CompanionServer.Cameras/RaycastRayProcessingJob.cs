using System.Threading;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace CompanionServer.Cameras;

public struct RaycastRayProcessingJob : IJobParallelFor
{
	public float3 cameraForward;

	public float farPlane;

	[Unity.Collections.ReadOnly]
	public NativeArray<RaycastHit> raycastHits;

	[Unity.Collections.ReadOnly]
	public NativeArray<int> colliderIds;

	[Unity.Collections.ReadOnly]
	public NativeArray<byte> colliderMaterials;

	[WriteOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<int> colliderHits;

	[WriteOnly]
	[NativeMatchesParallelForLength]
	public NativeArray<int> outputs;

	[NativeDisableParallelForRestriction]
	public NativeArray<int> foundCollidersIndex;

	[NativeDisableParallelForRestriction]
	public NativeArray<int> foundColliders;

	public void Execute(int index)
	{
		ref readonly RaycastHit @readonly = ref CompanionServer.Cameras.BurstUtil.GetReadonly(in raycastHits, index);
		int colliderId = @readonly.GetColliderId();
		bool num = colliderId != 0;
		byte b = 0;
		if (num)
		{
			int num2 = Interlocked.Increment(ref CompanionServer.Cameras.BurstUtil.Get(in foundCollidersIndex, 0));
			if (num2 <= foundColliders.Length)
			{
				foundColliders[num2 - 1] = colliderId;
			}
			int num3 = BinarySearch(colliderIds, colliderId);
			if (num3 >= 0)
			{
				b = colliderMaterials[num3];
				Interlocked.Increment(ref CompanionServer.Cameras.BurstUtil.Get(in colliderHits, num3));
			}
		}
		float num4 = (num ? @readonly.distance : farPlane);
		if (b == 7)
		{
			b = 0;
			num4 *= 1.1f;
		}
		float num5 = math.clamp(num4 / farPlane, 0f, 1f);
		float num6 = math.max(math.dot(cameraForward, @readonly.normal), 0f);
		ushort num7 = (ushort)(num5 * 1023f);
		byte b2 = (byte)(num6 * 63f);
		outputs[index] = (num7 >> 8 << 24) | ((num7 & 0xFF) << 16) | (b2 << 8) | b;
	}

	private static int BinarySearch(NativeArray<int> haystack, int needle)
	{
		int num = 0;
		int num2 = haystack.Length - 1;
		while (num <= num2)
		{
			int num3 = num + (num2 - num / 2);
			int num4 = Compare(haystack[num3], needle);
			if (num4 == 0)
			{
				return num3;
			}
			if (num4 < 0)
			{
				num = num3 + 1;
			}
			else
			{
				num2 = num3 - 1;
			}
		}
		return ~num;
	}

	private static int Compare(int x, int y)
	{
		if (x < y)
		{
			return -1;
		}
		if (x > y)
		{
			return 1;
		}
		return 0;
	}
}
