using Services;
using Singletons;
using UnityEngine;

namespace Characters
{
	public class AdventurerCharacterSpecificator : MonoBehaviour
	{
		[SerializeField]
		[GetComponent]
		private Character _character;

		private Stat.Values _statValue;

		private void Awake()
		{
			_statValue = new Stat.Values(new Stat.Value(Stat.Category.Percent, Stat.Kind.AttackDamage, Singleton<Service>.Instance.levelManager.currentChapter.currentStage.adventurerAttackDamageMultiplier), new Stat.Value(Stat.Category.Percent, Stat.Kind.Health, Singleton<Service>.Instance.levelManager.currentChapter.currentStage.adventurerHealthMultiplier));
			_character.stat.AttachValues(_statValue);
		}
	}
}
