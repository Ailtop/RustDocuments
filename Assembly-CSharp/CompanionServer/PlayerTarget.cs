using System;

namespace CompanionServer
{
	public struct PlayerTarget : IEquatable<PlayerTarget>
	{
		public ulong SteamId { get; }

		public PlayerTarget(ulong steamId)
		{
			SteamId = steamId;
		}

		public bool Equals(PlayerTarget other)
		{
			return SteamId == other.SteamId;
		}

		public override bool Equals(object obj)
		{
			object obj2;
			if ((obj2 = obj) is PlayerTarget)
			{
				PlayerTarget other = (PlayerTarget)obj2;
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return SteamId.GetHashCode();
		}

		public static bool operator ==(PlayerTarget left, PlayerTarget right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(PlayerTarget left, PlayerTarget right)
		{
			return !left.Equals(right);
		}
	}
}
