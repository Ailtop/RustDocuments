using UnityEngine;

namespace Characters.Gear.Synergy.Keywords
{
	public class March : SimpleStatBonusKeyword
	{
		[SerializeField]
		private double[] _statBonusByLevel;

		protected override double[] statBonusByLevel => _statBonusByLevel;

		protected override Stat.Category statCategory => Stat.Category.Percent;

		protected override Stat.Kind statKind => Stat.Kind.PhysicalAttackDamage;

		public override Key key => Key.March;

		protected override void UpdateBonus()
		{
			UpdateStat();
		}
	}
}
