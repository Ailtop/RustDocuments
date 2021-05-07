using System;
using System.Collections;
using Characters;
using Characters.Gear.Weapons;
using Services;
using Singletons;
using UnityEngine;

namespace Level
{
	public class DroppedSkulHead : MonoBehaviour
	{
		private const float _lootDistance = 1f;

		private const float _sqrLootDistance = 1f;

		[SerializeField]
		[GetComponent]
		private PoolObject _poolObject;

		[SerializeField]
		[GetComponent]
		private Rigidbody2D _rigidbody;

		private Character _player;

		private SkulHeadController _skulHeadController;

		private void OnEnable()
		{
			_rigidbody.gravityScale = 3f;
			_rigidbody.velocity = new Vector2(UnityEngine.Random.Range(-3f, 3f), UnityEngine.Random.Range(5f, 15f));
			_rigidbody.AddTorque(UnityEngine.Random.Range(0, 15) * (MMMaths.RandomBool() ? 1 : (-1)));
			_player = Singleton<Service>.Instance.levelManager.player;
			StartCoroutine(CUpdate());
			Weapon[] weapons = _player.playerComponents.inventory.weapon.weapons;
			foreach (Weapon weapon in weapons)
			{
				if (!(weapon == null) && weapon.name.Equals("Skul", StringComparison.OrdinalIgnoreCase))
				{
					_skulHeadController = weapon.equipped.GetComponent<SkulHeadController>();
					break;
				}
			}
		}

		private IEnumerator CUpdate()
		{
			float time = 0f;
			yield return Chronometer.global.WaitForSeconds(0.5f);
			while (!(_skulHeadController == null) && _skulHeadController.cooldown.stacks <= 0)
			{
				Vector3 center = Singleton<Service>.Instance.levelManager.player.collider.bounds.center;
				float sqrMagnitude = new Vector2(base.transform.position.x - center.x, base.transform.position.y - center.y).sqrMagnitude;
				time += Chronometer.global.deltaTime;
				if (time >= 0.5f && sqrMagnitude < 1f)
				{
					_skulHeadController.cooldown.time.remainTime = 0f;
					_poolObject.Despawn();
				}
				yield return null;
			}
			_poolObject.Despawn();
		}
	}
}
