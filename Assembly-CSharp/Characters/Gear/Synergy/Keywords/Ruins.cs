using UnityEngine;

namespace Characters.Gear.Synergy.Keywords
{
	public class Ruins : SimpleStatBonusKeyword
	{
		[SerializeField]
		private double[] _statBonusByLevel = new double[4] { 0.0, 0.3, 0.8, 2.0 };

		protected override double[] statBonusByLevel => _statBonusByLevel;

		protected override Stat.Category statCategory => Stat.Category.PercentPoint;

		protected override Stat.Kind statKind => Stat.Kind.EssenceCooldownSpeed;

		public override Key key => Key.Ruins;

		protected override void UpdateBonus()
		{
			UpdateStat();
		}
	}
}
