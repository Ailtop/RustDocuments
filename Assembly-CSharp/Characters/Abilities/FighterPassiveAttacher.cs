using Characters.Abilities.Customs;
using UnityEngine;

namespace Characters.Abilities
{
	public class FighterPassiveAttacher : AbilityAttacher
	{
		[SerializeField]
		private FighterPassive _fighterPassive;

		public bool rageReady => _fighterPassive.rageReady;

		public override void OnIntialize()
		{
			_fighterPassive.Initialize();
		}

		public override void StartAttach()
		{
			base.owner.ability.Add(_fighterPassive);
		}

		public override void StopAttach()
		{
			if (!(base.owner == null))
			{
				base.owner.ability.Remove(_fighterPassive);
			}
		}

		public void Rage()
		{
			_fighterPassive.AttachRage();
		}

		public override string ToString()
		{
			return this.GetAutoName();
		}
	}
}
