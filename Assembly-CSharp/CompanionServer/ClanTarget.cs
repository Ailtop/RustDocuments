using System;

namespace CompanionServer;

public struct ClanTarget : IEquatable<ClanTarget>
{
	public long ClanId { get; }

	public ClanTarget(long clanId)
	{
		ClanId = clanId;
	}

	public bool Equals(ClanTarget other)
	{
		return ClanId == other.ClanId;
	}

	public override bool Equals(object obj)
	{
		if (obj is ClanTarget other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (int)ClanId;
	}

	public static bool operator ==(ClanTarget left, ClanTarget right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(ClanTarget left, ClanTarget right)
	{
		return !left.Equals(right);
	}
}
