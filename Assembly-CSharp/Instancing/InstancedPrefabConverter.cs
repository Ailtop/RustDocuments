using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Instancing;

public static class InstancedPrefabConverter
{
	private static InstancedMeshCategory GetMeshCategory(string prefabPath)
	{
		if (prefabPath.StartsWith("assets/prefabs/building core"))
		{
			return InstancedMeshCategory.BuildingBlock;
		}
		if (prefabPath.StartsWith("assets/bundled/prefabs/autospawn/"))
		{
			return InstancedMeshCategory.Cliff;
		}
		return InstancedMeshCategory.Other;
	}

	public static InstancedPrefabConfig ExtractInstancedRenderers(GameObject prefab, uint prefabId, IEnumerable<InstancedMeshFilter> instancedFilters)
	{
		InstancedPrefabConfig instancedPrefabConfig = new InstancedPrefabConfig(prefabId);
		string text = StringPool.Get(prefabId);
		(from x in instancedFilters
			select x.RendererLOD into x
			where x != null
			select x).ToArray();
		(from x in instancedFilters
			select x.MeshRenderer into x
			where x != null
			select x).ToArray();
		(from x in instancedFilters
			select x.MeshLOD into x
			where x != null
			select x).ToArray();
		foreach (Transform allChild in TransformEx.GetAllChildren(prefab.transform))
		{
			if (allChild.name == "reflection" && !allChild.gameObject.activeSelf)
			{
				Debug.Log("Reflection probe in " + text);
			}
		}
		InstancedMeshCategory meshCategory = GetMeshCategory(text);
		foreach (InstancedMeshFilter instancedFilter in instancedFilters)
		{
			InstancedMeshConfig instancedMeshConfig = new InstancedMeshConfig
			{
				states = new List<InstancedLODState>()
			};
			instancedPrefabConfig.Meshes.Add(instancedMeshConfig);
			instancedFilter.Config = instancedMeshConfig;
			float num = 2500f;
			InstancedLODState instancedLODState = null;
			if (instancedFilter.MeshRenderer != null)
			{
				MeshRenderer meshRenderer = instancedFilter.MeshRenderer;
				if (!(meshRenderer == null))
				{
					instancedLODState = new InstancedLODState(meshRenderer.transform.localToWorldMatrix, meshRenderer, 0f, num, 0, 1, meshCategory);
					instancedMeshConfig.states.Add(instancedLODState);
				}
			}
			else if (instancedFilter.MeshLOD != null)
			{
				MeshLOD.State[] array = instancedFilter.MeshLOD.States.OrderBy((MeshLOD.State x) => x.distance).ToArray();
				for (int i = 0; i < array.Length; i++)
				{
					MeshLOD.State state = array[i];
					MeshRenderer component = instancedFilter.MeshLOD.GetComponent<MeshRenderer>();
					if (!(component == null))
					{
						Matrix4x4 localToWorldMatrix = component.transform.localToWorldMatrix;
						MeshLOD.State state2 = array.ElementAtOrDefault(i + 1);
						float minDistance = ((i == 0) ? 0f : state.distance);
						float maxDistance = state2?.distance ?? num;
						instancedLODState = new InstancedLODState(localToWorldMatrix, component, minDistance, maxDistance, i, array.Length, meshCategory);
						instancedMeshConfig.states.Add(instancedLODState);
					}
				}
			}
			else
			{
				if (!(instancedFilter.RendererLOD != null))
				{
					continue;
				}
				RendererLOD.State[] array2 = instancedFilter.RendererLOD.States.OrderBy((RendererLOD.State x) => x.distance).ToArray();
				for (int j = 0; j < array2.Length; j++)
				{
					RendererLOD.State state3 = array2[j];
					MeshRenderer meshRenderer2 = state3.renderer as MeshRenderer;
					if (!(meshRenderer2 == null))
					{
						Matrix4x4 localToWorldMatrix2 = meshRenderer2.transform.localToWorldMatrix;
						RendererLOD.State state4 = array2.ElementAtOrDefault(j + 1);
						float minDistance2 = ((j == 0) ? 0f : state3.distance);
						float maxDistance2 = state4?.distance ?? num;
						instancedLODState = new InstancedLODState(localToWorldMatrix2, meshRenderer2, minDistance2, maxDistance2, j, array2.Length, meshCategory);
						instancedMeshConfig.states.Add(instancedLODState);
					}
				}
			}
		}
		return instancedPrefabConfig;
	}
}
