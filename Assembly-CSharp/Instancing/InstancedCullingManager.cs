using Unity.Collections;

namespace Instancing;

public class InstancedCullingManager
{
	public NativeArray<int> RenderSliceIndexes;

	public NativeArray<int> PostCullingMeshCounts;
}
