using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Characters;

namespace Level.Npc.Prophecies
{
	public class ViolentBlow : Prophecy
	{
		private const double _damage = 50.0;

		public ViolentBlow(string key, Rarity rarity)
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
			owner.onGaveDamage = (GaveDamageDelegate)Delegate.Combine(owner.onGaveDamage, new GaveDamageDelegate(OnOwnerGaveDamage));
		}

		protected override void OnDeactivate()
		{
			Character owner = _owner;
			owner.onGaveDamage = (GaveDamageDelegate)Delegate.Remove(owner.onGaveDamage, new GaveDamageDelegate(OnOwnerGaveDamage));
		}

		private void OnOwnerGaveDamage(ITarget target, [In][IsReadOnly] ref Damage originalDamage, [In][IsReadOnly] ref Damage tookDamage, double damageDealt)
		{
			Damage damage = tookDamage;
			if (damage.amount >= 50.0)
			{
				Fulfil();
			}
		}

		protected override void Reset()
		{
		}
	}
}
