using System;

namespace Instancing;

public struct PrefabRenderKey : IEquatable<PrefabRenderKey>
{
	public uint PrefabId;

	public int Grade;

	public ulong Skin;

	public PrefabRenderKey(uint prefabId, int grade, ulong skin)
	{
		PrefabId = prefabId;
		Grade = grade;
		Skin = skin;
	}

	public bool Equals(PrefabRenderKey other)
	{
		if (PrefabId == other.PrefabId && Grade == other.Grade)
		{
			return Skin == other.Skin;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is PrefabRenderKey other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (int)(((PrefabId * 397) ^ (uint)Grade) * 397) ^ Skin.GetHashCode();
	}
}
