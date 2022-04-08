using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class MapLayerRenderer : SingletonComponent<MapLayerRenderer>
{
	private int? _underwaterLabFloorCount;

	public Camera renderCamera;

	public CameraEvent cameraEvent;

	public Material renderMaterial;

	private MapLayer? _currentlyRenderedLayer;

	private void RenderTrainLayer()
	{
		using CommandBuffer cb = BuildCommandBufferTrainTunnels();
		RenderImpl(cb);
	}

	private CommandBuffer BuildCommandBufferTrainTunnels()
	{
		CommandBuffer commandBuffer = new CommandBuffer
		{
			name = "TrainLayer Render"
		};
		foreach (DungeonGridCell dungeonGridCell in TerrainMeta.Path.DungeonGridCells)
		{
			if (dungeonGridCell.MapRenderers == null || dungeonGridCell.MapRenderers.Length == 0)
			{
				continue;
			}
			MeshRenderer[] mapRenderers = dungeonGridCell.MapRenderers;
			foreach (MeshRenderer meshRenderer in mapRenderers)
			{
				if (!(meshRenderer == null) && meshRenderer.TryGetComponent<MeshFilter>(out var component))
				{
					Mesh sharedMesh = component.sharedMesh;
					int subMeshCount = sharedMesh.subMeshCount;
					Matrix4x4 localToWorldMatrix = meshRenderer.transform.localToWorldMatrix;
					for (int j = 0; j < subMeshCount; j++)
					{
						commandBuffer.DrawMesh(sharedMesh, localToWorldMatrix, renderMaterial, j);
					}
				}
			}
		}
		return commandBuffer;
	}

	private void RenderUnderwaterLabs(int floor)
	{
		using CommandBuffer cb = BuildCommandBufferUnderwaterLabs(floor);
		RenderImpl(cb);
	}

	public int GetUnderwaterLabFloorCount()
	{
		if (_underwaterLabFloorCount.HasValue)
		{
			return _underwaterLabFloorCount.Value;
		}
		List<DungeonBaseInfo> dungeonBaseEntrances = TerrainMeta.Path.DungeonBaseEntrances;
		_underwaterLabFloorCount = ((dungeonBaseEntrances != null && dungeonBaseEntrances.Count > 0) ? dungeonBaseEntrances.Max((DungeonBaseInfo l) => l.Floors.Count) : 0);
		return _underwaterLabFloorCount.Value;
	}

	private CommandBuffer BuildCommandBufferUnderwaterLabs(int floor)
	{
		CommandBuffer commandBuffer = new CommandBuffer
		{
			name = "UnderwaterLabLayer Render"
		};
		foreach (DungeonBaseInfo dungeonBaseEntrance in TerrainMeta.Path.DungeonBaseEntrances)
		{
			if (dungeonBaseEntrance.Floors.Count <= floor)
			{
				continue;
			}
			foreach (DungeonBaseLink link in dungeonBaseEntrance.Floors[floor].Links)
			{
				if (link.MapRenderers == null || link.MapRenderers.Length == 0)
				{
					continue;
				}
				MeshRenderer[] mapRenderers = link.MapRenderers;
				foreach (MeshRenderer meshRenderer in mapRenderers)
				{
					if (!(meshRenderer == null) && meshRenderer.TryGetComponent<MeshFilter>(out var component))
					{
						Mesh sharedMesh = component.sharedMesh;
						int subMeshCount = sharedMesh.subMeshCount;
						Matrix4x4 localToWorldMatrix = meshRenderer.transform.localToWorldMatrix;
						for (int j = 0; j < subMeshCount; j++)
						{
							commandBuffer.DrawMesh(sharedMesh, localToWorldMatrix, renderMaterial, j);
						}
					}
				}
			}
		}
		return commandBuffer;
	}

	public void Render(MapLayer layer)
	{
		if (layer == _currentlyRenderedLayer)
		{
			return;
		}
		_currentlyRenderedLayer = layer;
		if (layer >= MapLayer.TrainTunnels)
		{
			if (layer == MapLayer.TrainTunnels)
			{
				RenderTrainLayer();
			}
			else
			{
				RenderUnderwaterLabs((int)(layer - 1));
			}
		}
	}

	private void RenderImpl(CommandBuffer cb)
	{
		double num = (double)World.Size * 1.5;
		renderCamera.orthographicSize = (float)num / 2f;
		renderCamera.RemoveAllCommandBuffers();
		renderCamera.AddCommandBuffer(cameraEvent, cb);
		renderCamera.Render();
		renderCamera.RemoveAllCommandBuffers();
	}

	public static MapLayerRenderer GetOrCreate()
	{
		if (SingletonComponent<MapLayerRenderer>.Instance != null)
		{
			return SingletonComponent<MapLayerRenderer>.Instance;
		}
		return GameManager.server.CreatePrefab("assets/prefabs/engine/maplayerrenderer.prefab", Vector3.zero, Quaternion.identity).GetComponent<MapLayerRenderer>();
	}
}
