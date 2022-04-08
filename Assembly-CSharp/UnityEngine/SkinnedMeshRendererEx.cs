namespace UnityEngine;

public static class SkinnedMeshRendererEx
{
	public static Transform FindRig(this SkinnedMeshRenderer renderer)
	{
		Transform parent = renderer.transform.parent;
		Transform transform = renderer.rootBone;
		while (transform != null && transform.parent != null && transform.parent != parent)
		{
			transform = transform.parent;
		}
		return transform;
	}
}
