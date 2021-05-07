using System;
using System.Collections;
using Characters;
using Characters.Abilities.Customs;
using FX;
using Singletons;
using UnityEngine;

namespace Level
{
	public class DroppedMummyGun : MonoBehaviour
	{
		private class PickupProxy : MonoBehaviour, IPickupable
		{
			public DroppedMummyGun droppedMummyGun;

			public void PickedUpBy(Character character)
			{
				droppedMummyGun.PickedUpBy(character);
			}
		}

		[SerializeField]
		private string _key;

		[Space]
		[SerializeField]
		private Collider2D _pickupTrigger;

		[SerializeField]
		private float _pickupDelay;

		[SerializeField]
		private float _startYVelocity;

		[SerializeField]
		private EffectInfo _effect;

		[SerializeField]
		private SoundInfo _sound;

		[Space]
		[SerializeField]
		[GetComponent]
		private PoolObject _poolObject;

		[SerializeField]
		[GetComponent]
		private Rigidbody2D _rigidbody;

		private MummyPassive _mummyPassive;

		public Rigidbody2D rigidbody => _rigidbody;

		public event Action onPickedUp;

		private void Awake()
		{
			_pickupTrigger.enabled = false;
			_pickupTrigger.gameObject.AddComponent<PickupProxy>().droppedMummyGun = this;
		}

		private void OnEnable()
		{
			_pickupTrigger.enabled = false;
			_rigidbody.gravityScale = 3f;
			_rigidbody.velocity = new Vector2(0f, _startYVelocity);
			StartCoroutine(CUpdatePickupable());
		}

		private IEnumerator CUpdatePickupable()
		{
			_pickupTrigger.enabled = false;
			yield return Chronometer.global.WaitForSeconds(_pickupDelay);
			_pickupTrigger.enabled = true;
		}

		public DroppedMummyGun Spawn(Vector3 position, MummyPassive mummyPassive)
		{
			DroppedMummyGun component = _poolObject.Spawn(position).GetComponent<DroppedMummyGun>();
			component._mummyPassive = mummyPassive;
			return component;
		}

		public void PickedUpBy(Character character)
		{
			if (_mummyPassive != null)
			{
				_mummyPassive.PickUpWeapon(_key);
			}
			_effect.Spawn(base.transform.position);
			PersistentSingleton<SoundManager>.Instance.PlaySound(_sound, base.transform.position);
			_poolObject.Despawn();
			this.onPickedUp?.Invoke();
		}
	}
}
