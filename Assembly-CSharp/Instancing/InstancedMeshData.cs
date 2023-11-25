using Unity.Mathematics;

namespace Instancing;

public struct InstancedMeshData
{
	public InstancedCullData CullData;

	public float4x4 LocalToWorld;
}
