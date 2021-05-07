using System.Collections;
using Characters.Gear.Weapons.Gauges;
using FX;
using Services;
using Singletons;
using UnityEngine;

namespace Level
{
	public class DroppedCell : MonoBehaviour
	{
		private const float _lootDistance = 3f;

		private const float _sqrLootDistance = 9f;

		[SerializeField]
		private EffectInfo _effect;

		[SerializeField]
		private SoundInfo _sound;

		[SerializeField]
		[GetComponent]
		private PoolObject _poolObject;

		[SerializeField]
		[GetComponent]
		private Rigidbody2D _rigidbody;

		private ValueGauge _prisonerGauge;

		private void OnEnable()
		{
			_rigidbody.gravityScale = 3f;
			_rigidbody.velocity = new Vector2(Random.Range(-3f, 3f), Random.Range(5f, 15f));
			_rigidbody.AddTorque(Random.Range(0, 15) * (MMMaths.RandomBool() ? 1 : (-1)));
			StartCoroutine(CUpdate());
		}

		private IEnumerator CUpdate()
		{
			float time = 0f;
			yield return Chronometer.global.WaitForSeconds(0.5f);
			while (true)
			{
				Vector3 center = Singleton<Service>.Instance.levelManager.player.collider.bounds.center;
				float sqrMagnitude = new Vector2(base.transform.position.x - center.x, base.transform.position.y - center.y).sqrMagnitude;
				time += Chronometer.global.deltaTime;
				if (time >= 0.5f && sqrMagnitude < 9f)
				{
					_prisonerGauge.Add(1f);
					_effect.Spawn(base.transform.position);
					PersistentSingleton<SoundManager>.Instance.PlaySound(_sound, base.transform.position);
					_poolObject.Despawn();
				}
				yield return null;
			}
		}

		public void Spawn(Vector3 postion, ValueGauge gauge)
		{
			_poolObject.Spawn(postion).GetComponent<DroppedCell>()._prisonerGauge = gauge;
		}
	}
}
