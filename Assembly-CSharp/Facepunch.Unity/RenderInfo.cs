using System;
using System.Collections.Generic;
using System.IO;
using Facepunch.Utility;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Rendering;

namespace Facepunch.Unity
{
	public static class RenderInfo
	{
		public struct RendererInstance
		{
			public bool IsVisible;

			public bool CastShadows;

			public bool Enabled;

			public bool RecieveShadows;

			public float Size;

			public float Distance;

			public int BoneCount;

			public int MaterialCount;

			public int VertexCount;

			public int TriangleCount;

			public int SubMeshCount;

			public int BlendShapeCount;

			public string RenderType;

			public string MeshName;

			public string ObjectName;

			public string EntityName;

			public uint EntityId;

			public bool UpdateWhenOffscreen;

			public int ParticleCount;

			public static RendererInstance From(Renderer renderer)
			{
				RendererInstance result = default(RendererInstance);
				result.IsVisible = renderer.isVisible;
				result.CastShadows = renderer.shadowCastingMode != ShadowCastingMode.Off;
				result.RecieveShadows = renderer.receiveShadows;
				result.Enabled = renderer.enabled && renderer.gameObject.activeInHierarchy;
				result.Size = renderer.bounds.size.magnitude;
				result.Distance = Vector3.Distance(renderer.bounds.center, Camera.main.transform.position);
				result.MaterialCount = renderer.sharedMaterials.Length;
				result.RenderType = renderer.GetType().Name;
				BaseEntity baseEntity = GameObjectEx.ToBaseEntity(renderer.gameObject);
				if ((bool)baseEntity)
				{
					result.EntityName = baseEntity.PrefabName;
					if (baseEntity.net != null)
					{
						result.EntityId = baseEntity.net.ID;
					}
				}
				else
				{
					result.ObjectName = TransformEx.GetRecursiveName(renderer.transform);
				}
				if (renderer is MeshRenderer)
				{
					result.BoneCount = 0;
					MeshFilter component = renderer.GetComponent<MeshFilter>();
					if ((bool)component)
					{
						result.ReadMesh(component.sharedMesh);
					}
				}
				if (renderer is SkinnedMeshRenderer)
				{
					SkinnedMeshRenderer skinnedMeshRenderer = renderer as SkinnedMeshRenderer;
					result.ReadMesh(skinnedMeshRenderer.sharedMesh);
					result.UpdateWhenOffscreen = skinnedMeshRenderer.updateWhenOffscreen;
				}
				if (renderer is ParticleSystemRenderer)
				{
					ParticleSystem component2 = renderer.GetComponent<ParticleSystem>();
					if ((bool)component2)
					{
						result.MeshName = component2.name;
						result.ParticleCount = component2.particleCount;
					}
				}
				return result;
			}

			public void ReadMesh(UnityEngine.Mesh mesh)
			{
				if (mesh == null)
				{
					MeshName = "<NULL>";
					return;
				}
				VertexCount = mesh.vertexCount;
				SubMeshCount = mesh.subMeshCount;
				BlendShapeCount = mesh.blendShapeCount;
				MeshName = mesh.name;
			}
		}

		public static void GenerateReport()
		{
			Renderer[] array = UnityEngine.Object.FindObjectsOfType<Renderer>();
			List<RendererInstance> list = new List<RendererInstance>();
			Renderer[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				RendererInstance item = RendererInstance.From(array2[i]);
				list.Add(item);
			}
			string text = string.Format(UnityEngine.Application.dataPath + "/../RenderInfo-{0:yyyy-MM-dd_hh-mm-ss-tt}.txt", DateTime.Now);
			string contents = JsonConvert.SerializeObject(list, Formatting.Indented);
			File.WriteAllText(text, contents);
			string text2 = UnityEngine.Application.streamingAssetsPath + "/RenderInfo.exe";
			string text3 = "\"" + text + "\"";
			Debug.Log("Launching " + text2 + " " + text3);
			Os.StartProcess(text2, text3);
		}
	}
}
