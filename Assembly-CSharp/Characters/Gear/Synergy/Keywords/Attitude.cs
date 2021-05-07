using UnityEngine;

namespace Characters.Gear.Synergy.Keywords
{
	public class Attitude : SimpleStatBonusKeyword
	{
		[Header("PerecentPoint, 0.1 => 10% 증가")]
		[SerializeField]
		private double[] _statBonusByLevel = new double[4] { 0.0, 0.3, 0.8, 2.0 };

		protected override double[] statBonusByLevel => _statBonusByLevel;

		public override Key key => Key.Attitude;

		protected override Stat.Category statCategory => Stat.Category.PercentPoint;

		protected override Stat.Kind statKind => Stat.Kind.ChargingSpeed;

		protected override void UpdateBonus()
		{
			UpdateStat();
		}
	}
}
