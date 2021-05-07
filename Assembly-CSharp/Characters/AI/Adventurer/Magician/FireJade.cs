using System.Collections;
using Characters.Operations;
using Characters.Operations.Attack;
using Characters.Operations.Fx;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Adventurer.Magician
{
	public class FireJade : MonoBehaviour
	{
		[SerializeField]
		private Character _character;

		[SerializeField]
		[Range(0f, 1f)]
		private float _startTiming;

		[SerializeField]
		private float _interval = 0.5f;

		[SerializeField]
		private float _lifeTime = 1.5f;

		[SerializeField]
		private TakeAim _takeAim;

		[SerializeField]
		private FireProjectile _fireProjectile;

		[SerializeField]
		[Subcomponent(typeof(SpawnEffect))]
		private SpawnEffect _spawnReadyEffect;

		[SerializeField]
		[Subcomponent(typeof(SpawnEffect))]
		private SpawnEffect _spawnFireEffect;

		[SerializeField]
		[Subcomponent(typeof(PlaySound))]
		private PlaySound _spawnSound;

		private void OnEnable()
		{
			_spawnReadyEffect.Initialize();
			_spawnFireEffect.Initialize();
			_fireProjectile.Initialize();
			StartCoroutine(CAttack());
			StartCoroutine(CHide());
		}

		private IEnumerator CAttack()
		{
			_spawnReadyEffect.Run(_character);
			yield return Chronometer.global.WaitForSeconds(_startTiming * _interval);
			while (true)
			{
				_takeAim.Run(_character);
				_fireProjectile.Run(_character);
				_spawnFireEffect.Run(_character);
				_spawnSound.Run(_character);
				yield return Chronometer.global.WaitForSeconds(_interval);
			}
		}

		private IEnumerator CHide()
		{
			yield return Chronometer.global.WaitForSeconds(_lifeTime);
			_spawnReadyEffect.Run(_character);
			base.gameObject.SetActive(false);
		}
	}
}
