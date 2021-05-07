using Characters;
using Characters.Abilities.CharacterStat;
using Services;
using Singletons;
using UnityEngine;

namespace Level.Altars
{
	public class BrutalityAltar : MonoBehaviour
	{
		[SerializeField]
		[GetComponent]
		private Altar _altar;

		[SerializeField]
		private StatBonusComponent _statBonus;

		private string _floatingText;

		private void Awake()
		{
			_altar.onDestroyed += OnAltarDestroyed;
			_floatingText = Lingua.GetLocalizedString("floating/altar/brutality");
		}

		private void OnAltarDestroyed()
		{
			Character player = Singleton<Service>.Instance.levelManager.player;
			Bounds bounds = player.collider.bounds;
			Singleton<Service>.Instance.floatingTextSpawner.SpawnBuff(_floatingText, new Vector2(bounds.center.x, bounds.max.y + 1f));
			player.ability.Add(_statBonus.ability);
		}
	}
}
