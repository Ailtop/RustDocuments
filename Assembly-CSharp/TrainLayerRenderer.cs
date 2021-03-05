using UnityEngine;
using UnityEngine.Rendering;

public class TrainLayerRenderer : SingletonComponent<TrainLayerRenderer>
{
	public Camera renderCamera;

	public Material renderMaterial;

	public CameraEvent cameraEvent;

	private bool _hasRendered;

	public void Render()
	{
		if (!_hasRendered)
		{
			using (CommandBuffer buffer = BuildCommandBuffer())
			{
				double num = (double)World.Size * 1.5;
				renderCamera.orthographicSize = (float)num / 2f;
				renderCamera.RemoveAllCommandBuffers();
				renderCamera.AddCommandBuffer(cameraEvent, buffer);
				renderCamera.Render();
				renderCamera.RemoveAllCommandBuffers();
			}
			_hasRendered = true;
		}
	}

	[ContextMenu("ForceRender")]
	public void ForceRender()
	{
		_hasRendered = false;
		Render();
	}

	private CommandBuffer BuildCommandBuffer()
	{
		CommandBuffer commandBuffer = new CommandBuffer
		{
			name = "TrainLayer Render"
		};
		DungeonCell[] array = Object.FindObjectsOfType<DungeonCell>();
		foreach (DungeonCell dungeonCell in array)
		{
			if (dungeonCell.MapRenderers == null || dungeonCell.MapRenderers.Length == 0)
			{
				continue;
			}
			MeshRenderer[] mapRenderers = dungeonCell.MapRenderers;
			foreach (MeshRenderer meshRenderer in mapRenderers)
			{
				MeshFilter component;
				if (!(meshRenderer == null) && meshRenderer.TryGetComponent<MeshFilter>(out component))
				{
					Mesh sharedMesh = component.sharedMesh;
					int subMeshCount = sharedMesh.subMeshCount;
					Matrix4x4 localToWorldMatrix = meshRenderer.transform.localToWorldMatrix;
					for (int k = 0; k < subMeshCount; k++)
					{
						commandBuffer.DrawMesh(sharedMesh, localToWorldMatrix, renderMaterial, k);
					}
				}
			}
		}
		return commandBuffer;
	}

	public static TrainLayerRenderer GetOrCreate()
	{
		if (SingletonComponent<TrainLayerRenderer>.Instance != null)
		{
			return SingletonComponent<TrainLayerRenderer>.Instance;
		}
		return GameManager.server.CreatePrefab("assets/prefabs/engine/trainlayerrenderer.prefab", Vector3.zero, Quaternion.identity).GetComponent<TrainLayerRenderer>();
	}
}
