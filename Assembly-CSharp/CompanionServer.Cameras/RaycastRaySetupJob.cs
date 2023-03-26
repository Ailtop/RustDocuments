using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace CompanionServer.Cameras;

public struct RaycastRaySetupJob : IJobParallelFor
{
	public float2 res;

	public float2 halfRes;

	public float aspectRatio;

	public float worldHeight;

	public float3 cameraPos;

	public quaternion cameraRot;

	public float nearPlane;

	public float farPlane;

	public int layerMask;

	public int sampleOffset;

	[Unity.Collections.ReadOnly]
	public NativeArray<int2> samplePositions;

	[WriteOnly]
	[NativeMatchesParallelForLength]
	public NativeArray<RaycastCommand> raycastCommands;

	public void Execute(int index)
	{
		int num;
		for (num = sampleOffset + index; num >= samplePositions.Length; num -= samplePositions.Length)
		{
		}
		float2 @float = (samplePositions[num] - halfRes) / res;
		float3 float2 = math.mul(v: new float3(@float.x * worldHeight * aspectRatio, @float.y * worldHeight, 1f), q: cameraRot);
		float3 float3 = cameraPos + float2 * nearPlane;
		raycastCommands[index] = new RaycastCommand(float3, float2, farPlane, layerMask);
	}
}
