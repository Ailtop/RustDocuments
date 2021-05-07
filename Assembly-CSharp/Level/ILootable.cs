using System;

namespace Level
{
	public interface ILootable
	{
		bool looted { get; }

		event Action onLoot;

		void Activate();
	}
}
