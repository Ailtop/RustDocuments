using System;

namespace CompanionServer;

public readonly struct EntityTarget : IEquatable<EntityTarget>
{
	public NetworkableId EntityId { get; }

	public EntityTarget(NetworkableId entityId)
	{
		EntityId = entityId;
	}

	public bool Equals(EntityTarget other)
	{
		return EntityId == other.EntityId;
	}

	public override bool Equals(object obj)
	{
		if (obj is EntityTarget other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return EntityId.GetHashCode();
	}

	public static bool operator ==(EntityTarget left, EntityTarget right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(EntityTarget left, EntityTarget right)
	{
		return !left.Equals(right);
	}
}
