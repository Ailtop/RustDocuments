using Characters;
using Characters.Abilities;
using Services;
using Singletons;
using UnityEngine;

namespace Level.Altars
{
	public class SteelAltar : MonoBehaviour
	{
		[SerializeField]
		[GetComponent]
		private Altar _altar;

		[SerializeField]
		private ShieldComponent _shieldComponent;

		private string _floatingText;

		private void Awake()
		{
			_altar.onDestroyed += OnAltarDestroyed;
			_floatingText = Lingua.GetLocalizedString("floating/altar/steel");
		}

		private void OnAltarDestroyed()
		{
			Character player = Singleton<Service>.Instance.levelManager.player;
			player.ability.Add(_shieldComponent.ability);
			Bounds bounds = player.collider.bounds;
			Singleton<Service>.Instance.floatingTextSpawner.SpawnBuff(_floatingText, new Vector2(bounds.center.x, bounds.max.y + 1f));
		}
	}
}
