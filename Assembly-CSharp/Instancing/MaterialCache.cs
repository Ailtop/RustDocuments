using System.Collections.Generic;
using UnityEngine;

namespace Instancing;

public class MaterialCache
{
	public Dictionary<Material, Material> modifiedMaterials = new Dictionary<Material, Material>();

	public Material EnableProceduralInstancing(Material material)
	{
		if (modifiedMaterials.TryGetValue(material, out var value))
		{
			return value;
		}
		value = new Material(material);
		value.enableInstancing = true;
		value.EnableKeyword("RUST_PROCEDURAL_INSTANCING");
		modifiedMaterials.Add(material, value);
		RequestTextureStreaming(material);
		return value;
	}

	private void RequestTextureStreaming(Material material)
	{
		RequestMipmap(material, "_MainTex");
		RequestMipmap(material, "_MetallicGlossMap");
		RequestMipmap(material, "_SpecGlossMap");
		RequestMipmap(material, "_BumpMap");
		RequestMipmap(material, "_OcclusionMap");
		RequestMipmap(material, "_EmissionMap");
		RequestMipmap(material, "_TransmissionMap");
		RequestMipmap(material, "_SubsurfaceMaskMap");
		RequestMipmap(material, "_TransmissionMaskMap");
		RequestMipmap(material, "_DetailMask");
		RequestMipmap(material, "_DetailOcclusionMap");
		RequestMipmap(material, "_BiomeLayer_TintMask");
		RequestMipmap(material, "_WetnessLayer_Mask");
		RequestMipmap(material, "_DetailAlbedoMap");
		RequestMipmap(material, "_DetailMetallicGlossMap");
		RequestMipmap(material, "_DetailNormalMap");
		RequestMipmap(material, "_DetailTintMap");
		RequestMipmap(material, "_DetailBlendMaskMap");
	}

	private void RequestMipmap(Material material, string textureName)
	{
		if (material.HasTexture(textureName))
		{
			Texture2D texture2D = material.GetTexture(textureName) as Texture2D;
			if (texture2D != null)
			{
				texture2D.requestedMipmapLevel = 0;
			}
		}
	}

	public void FreeMemory()
	{
		foreach (Material value in modifiedMaterials.Values)
		{
			Object.DestroyImmediate(value);
		}
		modifiedMaterials = new Dictionary<Material, Material>();
	}
}
