using Unity.Mathematics;

namespace Instancing;

public struct GridJobData
{
	public int GridId;

	public int StartIndex;

	public int Count;

	public int Capacity;

	public float3 MinBounds;

	public float3 MaxBounds;

	public bool CanBeFrustumCulled;

	public bool CanBeDistanceCulled;
}
