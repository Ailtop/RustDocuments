using System;
using Characters;

namespace Level.Npc.Prophecies
{
	public class PlainVanilla : Prophecy
	{
		public PlainVanilla(string key, Rarity rarity)
			: base(key, rarity)
		{
		}//IL_0002: Unknown result type (might be due to invalid IL or missing references)


		protected override void GetReward()
		{
			DropGear();
		}

		protected override void OnActivate()
		{
			Character owner = _owner;
			owner.onKilled = (Character.OnKilledDelegate)Delegate.Combine(owner.onKilled, new Character.OnKilledDelegate(OnOwnerKilled));
		}

		protected override void OnDeactivate()
		{
			Character owner = _owner;
			owner.onKilled = (Character.OnKilledDelegate)Delegate.Remove(owner.onKilled, new Character.OnKilledDelegate(OnOwnerKilled));
		}

		private void OnOwnerKilled(ITarget target, ref Damage damage)
		{
			if (!(target.character == null) && target.character.key == Key.Yggdrasil && _owner.playerComponents.inventory.weapon.polymorphOrCurrent.name.Equals("Skul", StringComparison.InvariantCultureIgnoreCase))
			{
				Fulfil();
			}
		}

		protected override void Reset()
		{
		}
	}
}
