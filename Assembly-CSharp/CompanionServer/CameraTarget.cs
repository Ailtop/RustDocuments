using System;

namespace CompanionServer;

public readonly struct CameraTarget : IEquatable<CameraTarget>
{
	public NetworkableId EntityId { get; }

	public CameraTarget(NetworkableId entityId)
	{
		EntityId = entityId;
	}

	public bool Equals(CameraTarget other)
	{
		return EntityId == other.EntityId;
	}

	public override bool Equals(object obj)
	{
		if (obj is CameraTarget other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return EntityId.GetHashCode();
	}

	public static bool operator ==(CameraTarget left, CameraTarget right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(CameraTarget left, CameraTarget right)
	{
		return !left.Equals(right);
	}
}
