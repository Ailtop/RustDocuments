using System.Collections;
using Characters;
using Characters.Actions;
using FX;
using Services;
using Singletons;
using UnityEngine;

namespace Level
{
	public class DroppedManatechPart : MonoBehaviour, IPickupable
	{
		private class PickupProxy : MonoBehaviour, IPickupable
		{
			public DroppedManatechPart part;

			public void PickedUpBy(Character character)
			{
				part.PickedUpBy(character);
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

		private Character _player;

		public PoolObject poolObject => _poolObject;

		public float cooldownReducingAmount { get; set; }

		private void Awake()
		{
			_pickupTrigger.enabled = false;
			_pickupTrigger.gameObject.AddComponent<PickupProxy>().part = this;
		}

		private void OnEnable()
		{
			_rigidbody.gravityScale = 3f;
			float num = Random.Range(5f, 10f);
			if (MMMaths.RandomBool())
			{
				num *= -1f;
			}
			_rigidbody.velocity = new Vector2(num, Random.Range(5f, 20f));
			_rigidbody.AddTorque(Random.Range(0, 20) * (MMMaths.RandomBool() ? 1 : (-1)));
			_player = Singleton<Service>.Instance.levelManager.player;
			StartCoroutine(CUpdatePickupable());
		}

		private IEnumerator CUpdatePickupable()
		{
			_pickupTrigger.enabled = false;
			yield return Chronometer.global.WaitForSeconds(0.5f);
			_pickupTrigger.enabled = true;
		}

		public void PickedUpBy(Character character)
		{
			foreach (Action action in _player.actions)
			{
				if (action.type == Action.Type.Skill && action.cooldown.time != null)
				{
					action.cooldown.time.remainTime -= cooldownReducingAmount;
				}
			}
			_effect.Spawn(base.transform.position);
			PersistentSingleton<SoundManager>.Instance.PlaySound(_sound, base.transform.position);
			_poolObject.Despawn();
		}
	}
}
