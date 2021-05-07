using UnityEngine;

namespace Characters.Gear.Synergy.Keywords
{
	public class Mutation : SimpleStatBonusKeyword
	{
		[SerializeField]
		private double[] _statBonusByLevel;

		protected override double[] statBonusByLevel => _statBonusByLevel;

		protected override Stat.Category statCategory => Stat.Category.PercentPoint;

		protected override Stat.Kind statKind => Stat.Kind.SwapCooldownSpeed;

		public override Key key => Key.Mutation;

		protected override void UpdateBonus()
		{
			UpdateStat();
		}
	}
}
