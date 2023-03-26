using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace CompanionServer.Cameras;

public struct RaycastSamplePositionsJob : IJob
{
	public int2 res;

	public Random random;

	public NativeArray<int2> positions;

	public void Execute()
	{
		int num = 0;
		for (int i = 0; i < res.y; i++)
		{
			for (int j = 0; j < res.x; j++)
			{
				positions[num++] = new int2(j, i);
			}
		}
		for (num = res.x * res.y - 1; num >= 1; num--)
		{
			int num2 = random.NextInt(num + 1);
			ref NativeArray<int2> reference = ref positions;
			int index = num;
			ref NativeArray<int2> reference2 = ref positions;
			int index2 = num2;
			int2 @int = positions[num2];
			int2 int2 = positions[num];
			int2 int4 = (reference[index] = @int);
			int4 = (reference2[index2] = int2);
		}
	}
}
