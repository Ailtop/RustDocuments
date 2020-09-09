using System;

namespace CompanionServer
{
	public struct EntityTarget : IEquatable<EntityTarget>
	{
		public uint EntityId
		{
			get;
		}

		public EntityTarget(uint entityId)
		{
			EntityId = entityId;
		}

		public bool Equals(EntityTarget other)
		{
			return EntityId == other.EntityId;
		}

		public override bool Equals(object obj)
		{
			object obj2;
			if ((obj2 = obj) is EntityTarget)
			{
				EntityTarget other = (EntityTarget)obj2;
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (int)EntityId;
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
}
