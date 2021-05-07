using System;
using System.Collections;
using Data;
using FX;
using Singletons;
using UnityEngine;

namespace Level
{
	[RequireComponent(typeof(PoolObject), typeof(Rigidbody2D))]
	public class CurrencyParticle : MonoBehaviour
	{
		private static readonly Vector2 _minVelocity = new Vector2(-4f, 7f);

		private static readonly Vector2 _maxVelocity = new Vector2(4f, 17f);

		private const float _minTorque = -10f;

		private const float _maxTorque = 10f;

		[Header("Required")]
		[SerializeField]
		[GetComponent]
		private PoolObject _poolObject;

		[SerializeField]
		[GetComponent]
		private Collider2D _collider;

		[SerializeField]
		[GetComponent]
		private Rigidbody2D _rigidbody;

		[Header("FX")]
		[SerializeField]
		private EffectInfo _effect;

		[SerializeField]
		private SoundInfo _sound;

		[NonSerialized]
		public GameData.Currency.Type currencyType;

		[NonSerialized]
		public int currencyAmount;

		private void OnEnable()
		{
			_collider.isTrigger = false;
			_rigidbody.gravityScale = 3f;
			_rigidbody.velocity = MMMaths.RandomVector2(_minVelocity, _maxVelocity);
			_rigidbody.AddTorque(UnityEngine.Random.Range(-10f, 10f));
			StartCoroutine(CUpdate());
		}

		private void OnDisable()
		{
			GameData.Currency.currencies[currencyType].Earn(currencyAmount);
		}

		public void SetForce(Vector2 force)
		{
			if (!(Mathf.Abs(force.x) < 1f) && !(Mathf.Abs(force.y) < 1f))
			{
				force.y = Mathf.Abs(force.y);
				force *= 4f;
				force = Quaternion.AngleAxis(UnityEngine.Random.Range(-15f, 15f), Vector3.forward) * force * UnityEngine.Random.Range(0.8f, 1.2f);
				force.y += 10f;
				_rigidbody.velocity = Vector2.zero;
				_rigidbody.angularVelocity = 0f;
				_rigidbody.AddForce(force * UnityEngine.Random.Range(0.5f, 1f), ForceMode2D.Impulse);
				_rigidbody.AddTorque(UnityEngine.Random.Range(-0.5f, 0.5f), ForceMode2D.Impulse);
			}
		}

		private IEnumerator CUpdate()
		{
			yield return Chronometer.global.WaitForSeconds(UnityEngine.Random.Range(0.9f, 1.1f));
			PersistentSingleton<SoundManager>.Instance.PlaySound(_sound, base.transform.position);
			_effect.Spawn(base.transform.position);
			_poolObject.Despawn();
		}
	}
}
