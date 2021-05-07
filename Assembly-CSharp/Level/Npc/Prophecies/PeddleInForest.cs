using System;
using Characters;
using Services;
using Singletons;

namespace Level.Npc.Prophecies
{
	public class PeddleInForest : Prophecy
	{
		public PeddleInForest(string key, Rarity rarity)
			: base(key, rarity)
		{
		}//IL_0002: Unknown result type (might be due to invalid IL or missing references)


		protected override void GetReward()
		{
		}

		protected override void OnActivate()
		{
			Character owner = _owner;
			owner.onKilled = (Character.OnKilledDelegate)Delegate.Combine(owner.onKilled, new Character.OnKilledDelegate(OnOwnerKilled));
			Singleton<Service>.Instance.levelManager.onChapterLoaded += OnChapterLoaded;
		}

		protected override void OnDeactivate()
		{
			Character owner = _owner;
			owner.onKilled = (Character.OnKilledDelegate)Delegate.Remove(owner.onKilled, new Character.OnKilledDelegate(OnOwnerKilled));
			Singleton<Service>.Instance.levelManager.onChapterLoaded -= OnChapterLoaded;
		}

		private void OnChapterLoaded()
		{
			if (Singleton<Service>.Instance.levelManager.currentChapter.type == Chapter.Type.Chapter2)
			{
				Fulfil();
			}
		}

		private void OnOwnerKilled(ITarget target, ref Damage damage)
		{
			if (!(target.damageable == null) && target.character.key != Key.BrutalityAltar && target.character.key != Key.EnergyAltar && target.character.key != Key.RageAltar && target.character.key != Key.SteelAltar)
			{
				Deactivate();
			}
		}

		protected override void Reset()
		{
		}
	}
}
