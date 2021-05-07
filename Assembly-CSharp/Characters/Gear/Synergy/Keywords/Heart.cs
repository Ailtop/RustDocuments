using System.Collections;
using UnityEngine;

namespace Characters.Gear.Synergy.Keywords
{
	public class Heart : SimpleStatBonusKeyword
	{
		[SerializeField]
		private double[] _statBonusByLevel = new double[6] { 0.0, 10.0, 20.0, 40.0, 80.0, 150.0 };

		protected override double[] statBonusByLevel => _statBonusByLevel;

		protected override Stat.Category statCategory => Stat.Category.Constant;

		protected override Stat.Kind statKind => Stat.Kind.Health;

		public override Key key => Key.Heart;

		protected override IList valuesByLevel => _statBonusByLevel;

		protected override void UpdateBonus()
		{
			UpdateStat();
		}
	}
}
