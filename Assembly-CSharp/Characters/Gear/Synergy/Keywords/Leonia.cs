using System.Collections;
using UnityEngine;

namespace Characters.Gear.Synergy.Keywords
{
	public class Leonia : SimpleStatBonusKeyword
	{
		[SerializeField]
		private double[] _statBonusByLevel;

		protected override double[] statBonusByLevel => _statBonusByLevel;

		protected override Stat.Category statCategory => Stat.Category.PercentPoint;

		protected override Stat.Kind statKind => Stat.Kind.TakingHealAmount;

		public override Key key => Key.Leonia;

		protected override IList valuesByLevel => _statBonusByLevel;

		protected override void UpdateBonus()
		{
			UpdateStat();
		}
	}
}
