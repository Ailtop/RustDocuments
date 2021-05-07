using System.Collections;
using Characters;
using Characters.Abilities.Customs;
using FX;
using Singletons;
using UnityEngine;

namespace Level
{
	public class DroppedGhoulFlesh : MonoBehaviour
	{
		private class PickupProxy : MonoBehaviour, IPickupable
		{
			public DroppedGhoulFlesh ghoulFlesh;

			public void PickedUpBy(Character character)
			{
				ghoulFlesh.PickedUpBy(character);
			}
		}

		[SerializeField]
		private Collider2D _pickupTrigger;

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

		private GhoulPassive _ghoulPassive;

		private void Awake()
		{
			_pickupTrigger.enabled = false;
			_pickupTrigger.gameObject.AddComponent<PickupProxy>().ghoulFlesh = this;
		}

		private void OnEnable()
		{
			_pickupTrigger.enabled = false;
			_rigidbody.gravityScale = 3f;
			_rigidbody.velocity = new Vector2(Random.Range(-3f, 3f), Random.Range(5f, 15f));
			_rigidbody.AddTorque(Random.Range(0, 15) * (MMMaths.RandomBool() ? 1 : (-1)));
			StartCoroutine(CUpdatePickupable());
		}

		private IEnumerator CUpdatePickupable()
		{
			_pickupTrigger.enabled = false;
			yield return Chronometer.global.WaitForSeconds(0.5f);
			_pickupTrigger.enabled = true;
		}

		public void Spawn(Vector3 postion, GhoulPassive ghoulPassive)
		{
			_poolObject.Spawn(postion).GetComponent<DroppedGhoulFlesh>()._ghoulPassive = ghoulPassive;
		}

		public void PickedUpBy(Character character)
		{
			_ghoulPassive.AddStack();
			_effect.Spawn(base.transform.position);
			PersistentSingleton<SoundManager>.Instance.PlaySound(_sound, base.transform.position);
			_poolObject.Despawn();
		}
	}
}
