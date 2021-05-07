using System;
using System.Collections;
using Characters;
using Characters.Abilities;
using Services;
using Singletons;
using UnityEngine;

namespace Level.Altars
{
	public class EnergyAltar : MonoBehaviour
	{
		[SerializeField]
		[GetComponent]
		private Altar _altar;

		[SerializeField]
		private PoolObject _effect;

		[SerializeField]
		private HealComponent _healComponent;

		private string _floatingText;

		private void Awake()
		{
			StartCoroutine(CHeal());
			_altar.onDestroyed += OnAltarDestroyed;
			_floatingText = Lingua.GetLocalizedString("floating/altar/energy");
		}

		private void OnAltarDestroyed()
		{
			Character player = Singleton<Service>.Instance.levelManager.player;
			player.ability.Add(_healComponent.ability);
			Bounds bounds = player.collider.bounds;
			Singleton<Service>.Instance.floatingTextSpawner.SpawnBuff(_floatingText, new Vector2(bounds.center.x, bounds.max.y + 1f));
			StopCoroutine("CHeal");
		}

		private IEnumerator CHeal()
		{
			while (true)
			{
				yield return Chronometer.global.WaitForSeconds(3f);
				foreach (Character character in _altar.characters)
				{
					if (!character.health.dead)
					{
						character.health.Heal(Math.Max(5.0, character.health.maximumHealth * 0.0666));
					}
				}
			}
		}
	}
}
