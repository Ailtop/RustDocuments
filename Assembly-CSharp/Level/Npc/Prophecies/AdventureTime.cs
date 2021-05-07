using System;
using Services;
using Singletons;

namespace Level.Npc.Prophecies
{
	public class AdventureTime : Prophecy
	{
		public AdventureTime(string key, Rarity rarity)
			: base(key, rarity)
		{
		}//IL_0002: Unknown result type (might be due to invalid IL or missing references)


		protected override void GetReward()
		{
			DropGear();
		}

		protected override void OnActivate()
		{
			Singleton<Service>.Instance.levelManager.onMapChangedAndFadedIn += OnMapChanged;
		}

		protected override void OnDeactivate()
		{
			Singleton<Service>.Instance.levelManager.onMapChangedAndFadedIn -= OnMapChanged;
		}

		private void OnMapChanged(Map old, Map @new)
		{
			new NotImplementedException();
		}

		protected override void Reset()
		{
		}
	}
}
