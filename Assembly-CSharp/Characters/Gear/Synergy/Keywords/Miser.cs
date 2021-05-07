using System.Collections;
using Data;
using UnityEngine;

namespace Characters.Gear.Synergy.Keywords
{
	public class Miser : Keyword
	{
		[SerializeField]
		private float[] _goldBonusByLevel;

		public override Key key => Key.Miser;

		protected override IList valuesByLevel => _goldBonusByLevel;

		protected override void Initialize()
		{
		}

		protected override void UpdateBonus()
		{
			GameData.Currency.currencies[GameData.Currency.Type.Gold].multiplier.AddOrUpdate(this, _goldBonusByLevel[base.level] * 0.01f);
		}

		protected override void OnAttach()
		{
			GameData.Currency.currencies[GameData.Currency.Type.Gold].multiplier.AddOrUpdate(this, _goldBonusByLevel[base.level] * 0.01f);
		}

		protected override void OnDetach()
		{
			GameData.Currency.currencies[GameData.Currency.Type.Gold].multiplier.Remove(this);
		}
	}
}
