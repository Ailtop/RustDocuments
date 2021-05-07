using UnityEngine;

namespace Characters.Gear.Synergy.Keywords
{
	public class Tactics : SimpleStatBonusKeyword
	{
		[SerializeField]
		private double[] _statBonusByLevel;

		protected override double[] statBonusByLevel => _statBonusByLevel;

		protected override Stat.Category statCategory => Stat.Category.Percent;

		protected override Stat.Kind statKind => Stat.Kind.MagicAttackDamage;

		public override Key key => Key.Tactics;

		protected override void UpdateBonus()
		{
			UpdateStat();
		}
	}
}
