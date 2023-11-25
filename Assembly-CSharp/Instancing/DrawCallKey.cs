using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Instancing;

public struct DrawCallKey : IEquatable<DrawCallKey>, IComparable<DrawCallKey>, IComparable
{
	public Material Material;

	public ShadowCastingMode ShadowMode;

	public bool ReceiveShadows;

	public LightProbeUsage LightProbes;

	public DrawCallKey(Material material, ShadowCastingMode shadowMode, bool receiveShadows, LightProbeUsage lightProbes)
	{
		Material = material;
		ShadowMode = shadowMode;
		ReceiveShadows = receiveShadows;
		LightProbes = lightProbes;
	}

	public int CompareTo(DrawCallKey other)
	{
		return GetHashCode().CompareTo(other.GetHashCode());
	}

	public int CompareTo(object obj)
	{
		if (obj == null)
		{
			return 1;
		}
		if (obj is DrawCallKey other)
		{
			return CompareTo(other);
		}
		throw new ArgumentException("Object must be 'DrawCallKey'");
	}

	public bool Equals(DrawCallKey other)
	{
		if (Material == other.Material && ShadowMode == other.ShadowMode && ReceiveShadows == other.ReceiveShadows)
		{
			return LightProbes == other.LightProbes;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is DrawCallKey other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Material?.GetHashCode() ?? 0, ShadowMode, ReceiveShadows, LightProbes);
	}

	public static bool operator ==(DrawCallKey a, DrawCallKey b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(DrawCallKey a, DrawCallKey b)
	{
		return !(a == b);
	}
}
