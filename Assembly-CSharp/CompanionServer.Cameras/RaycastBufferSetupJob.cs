using Unity.Collections;
using Unity.Jobs;

namespace CompanionServer.Cameras;

public struct RaycastBufferSetupJob : IJob
{
	public NativeArray<int> colliderIds;

	public NativeArray<byte> colliderMaterials;

	[WriteOnly]
	public NativeArray<int> colliderHits;

	public void Execute()
	{
		if (colliderIds.Length > 1)
		{
			SortByAscending(ref colliderIds, ref colliderMaterials, 0, colliderIds.Length - 1);
		}
		for (int i = 0; i < colliderHits.Length; i++)
		{
			colliderHits[i] = 0;
		}
	}

	private static void SortByAscending(ref NativeArray<int> colliderIds, ref NativeArray<byte> colliderMaterials, int leftIndex, int rightIndex)
	{
		int i = leftIndex;
		int num = rightIndex;
		int num2 = colliderIds[leftIndex];
		while (i <= num)
		{
			for (; colliderIds[i] < num2; i++)
			{
			}
			while (colliderIds[num] > num2)
			{
				num--;
			}
			if (i <= num)
			{
				int index = i;
				int index2 = num;
				int num3 = colliderIds[num];
				int num4 = colliderIds[i];
				int num6 = (colliderIds[index] = num3);
				num6 = (colliderIds[index2] = num4);
				num4 = i;
				num3 = num;
				byte b = colliderMaterials[num];
				byte b2 = colliderMaterials[i];
				byte b4 = (colliderMaterials[num4] = b);
				b4 = (colliderMaterials[num3] = b2);
				i++;
				num--;
			}
		}
		if (leftIndex < num)
		{
			SortByAscending(ref colliderIds, ref colliderMaterials, leftIndex, num);
		}
		if (i < rightIndex)
		{
			SortByAscending(ref colliderIds, ref colliderMaterials, i, rightIndex);
		}
	}
}
