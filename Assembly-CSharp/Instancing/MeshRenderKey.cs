using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Instancing;

public struct MeshRenderKey : IEquatable<MeshRenderKey>
{
	public Mesh Mesh;

	public Material[] Materials;

	public ShadowCastingMode CastShadows;

	public bool RecieveShadows;

	public LightProbeUsage LightProbeUsages;

	public MeshRenderKey(Mesh mesh, Material[] materials, ShadowCastingMode castShadows, bool recieveShadows, LightProbeUsage lightProbes)
	{
		Mesh = mesh;
		Materials = materials;
		CastShadows = castShadows;
		RecieveShadows = recieveShadows;
		LightProbeUsages = lightProbes;
	}

	public bool Equals(MeshRenderKey other)
	{
		if (Mesh != other.Mesh || CastShadows != other.CastShadows || RecieveShadows != other.RecieveShadows || LightProbeUsages != other.LightProbeUsages)
		{
			return false;
		}
		if (Materials == null || other.Materials == null)
		{
			return Materials == other.Materials;
		}
		for (int i = 0; i < Materials.Length; i++)
		{
			if (Materials[i] != other.Materials[i])
			{
				return false;
			}
		}
		return true;
	}

	public override bool Equals(object obj)
	{
		if (obj is MeshRenderKey other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (17 * 31 + Mesh?.GetHashCode()).GetValueOrDefault();
	}
}
