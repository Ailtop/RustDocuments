using UnityEngine;

namespace Characters.Gear.Synergy.Keywords
{
	public class Sorcery : SimpleStatBonusKeyword
	{
		[SerializeField]
		private double[] _statBonusByLevel;

		protected override double[] statBonusByLevel => _statBonusByLevel;

		protected override Stat.Category statCategory => Stat.Category.PercentPoint;

		protected override Stat.Kind statKind => Stat.Kind.SkillCooldownSpeed;

		public override Key key => Key.Sorcery;

		protected override void UpdateBonus()
		{
			UpdateStat();
		}
	}
}
