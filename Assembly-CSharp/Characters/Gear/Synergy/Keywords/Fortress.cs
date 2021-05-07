using System.Collections;
using Characters.Abilities;
using UnityEngine;

namespace Characters.Gear.Synergy.Keywords
{
	public class Fortress : Keyword
	{
		[SerializeField]
		private int _refreshInterval;

		[SerializeField]
		private float[] _shieldByLevel;

		[Information("duration은 _refreshInterval + 1로 설정하는 것을 권장", InformationAttribute.InformationType.Info, false)]
		[SerializeField]
		private Characters.Abilities.Shield _shield;

		public override Key key => Key.Fortress;

		protected override IList valuesByLevel => _shieldByLevel;

		protected override void Initialize()
		{
		}

		protected override void UpdateBonus()
		{
			_shield.amount = _shieldByLevel[base.level];
		}

		protected override void OnAttach()
		{
			StartCoroutine("CRefreshShield");
		}

		protected override void OnDetach()
		{
			StopCoroutine("CRefreshShield");
		}

		private IEnumerator CRefreshShield()
		{
			yield return null;
			while (true)
			{
				base.character.ability.Add(_shield);
				yield return base.character.chronometer.master.WaitForSeconds(_refreshInterval);
			}
		}
	}
}
