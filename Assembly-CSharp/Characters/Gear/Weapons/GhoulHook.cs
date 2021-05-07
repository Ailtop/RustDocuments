using System;
using System.Collections;
using Characters.Movements;
using FX;
using PhysicsUtils;
using Singletons;
using UnityEngine;

namespace Characters.Gear.Weapons
{
	public class GhoulHook : MonoBehaviour
	{
		private NonAllocCaster _caster = new NonAllocCaster(1);

		[SerializeField]
		private Weapon _weapon;

		[Space]
		[SerializeField]
		private Transform _fireOrigin;

		[SerializeField]
		private Transform _pullOrigin;

		[SerializeField]
		private Transform _flyOrigin;

		[Space]
		[SerializeField]
		private SpriteRenderer _chain;

		[SerializeField]
		private SpriteRenderer _head;

		[Header("Fire")]
		[SerializeField]
		private float _speed;

		[SerializeField]
		private float _distance;

		[SerializeField]
		private float _minDistanceForPlatform;

		[Header("Pull")]
		[SerializeField]
		private float _pullDelay;

		[SerializeField]
		private float _pullSpeed;

		[SerializeField]
		[Tooltip("Pull Collider의 너비와 각도는 체인에 맞게 자동으로 조정됩니다. 높이만 설정하세요.")]
		private BoxCollider2D _pullCollider;

		[Header("Fly")]
		[SerializeField]
		private float _flyDelay;

		[SerializeField]
		private float _flySpeed;

		[SerializeField]
		[Tooltip("Fly 상태가 지속될 수 있는 최대 시간입니다. 이 시간이 넘어가면 도착여부에 관계없이 Fly가 끝납니다.")]
		private float _flyTimeout;

		[SerializeField]
		[Tooltip("Fly가 끝날 때 Vertical Velocity를 몇으로 설정하지를 정합니다. Fly가 끝나면서 살짝 뛰어오르는 연출을 위해 사용합니다.")]
		private float _flyEndVerticalVelocity;

		[SerializeField]
		[Tooltip("Fly가 끝날 때 사운드를 재생합니다.")]
		private SoundInfo _flyEndSound;

		[SerializeField]
		private Movement.Config _flyMovmentConfig;

		private Transform _origin;

		public event Action onTerrainHit;

		public event Action onExpired;

		public event Action onPullEnd;

		public event Action onFlyEnd;

		private void Awake()
		{
			_chain.transform.parent = null;
			_head.transform.parent = null;
			_chain.gameObject.SetActive(false);
			_head.gameObject.SetActive(false);
		}

		private void LateUpdate()
		{
			if (_chain.gameObject.activeSelf)
			{
				_chain.transform.position = _origin.position;
				float num = Vector2.Distance(_chain.transform.position, _head.transform.position);
				Vector2 size = _pullCollider.size;
				size.x = num + 0.5f;
				_pullCollider.size = size;
				Vector2 offset = _pullCollider.offset;
				offset.x = num * 0.5f;
				if (_weapon.owner.lookingDirection == Character.LookingDirection.Left)
				{
					offset.x *= -1f;
				}
				_pullCollider.offset = offset;
				size = _chain.size;
				size.x = num;
				_chain.size = size;
				Vector3 right = _head.transform.position - _chain.transform.position;
				_chain.transform.right = right;
				_pullCollider.transform.right = right;
			}
		}

		private void OnDisable()
		{
			_weapon.owner.movement.configs.Remove(_flyMovmentConfig);
			_chain.gameObject.SetActive(false);
			_head.gameObject.SetActive(false);
		}

		private void OnDestroy()
		{
			UnityEngine.Object.Destroy(_chain.gameObject);
			UnityEngine.Object.Destroy(_head.gameObject);
		}

		public void Fire()
		{
			StopCoroutine("CFire");
			StartCoroutine("CFire");
		}

		private IEnumerator CFire()
		{
			_origin = _fireOrigin;
			_chain.gameObject.SetActive(true);
			_head.gameObject.SetActive(true);
			_head.transform.position = _origin.transform.position;
			Vector3 lossyScale = _weapon.owner.transform.lossyScale;
			_head.transform.localScale = base.transform.lossyScale;
			float traveled = 0f;
			while (traveled < _distance)
			{
				yield return null;
				_caster.contactFilter.SetLayerMask(Layers.terrainMaskForProjectile);
				float num = _speed * _weapon.owner.chronometer.animation.deltaTime;
				traveled += num;
				if (_weapon.owner.lookingDirection == Character.LookingDirection.Left)
				{
					num *= -1f;
				}
				Vector2 right = Vector2.right;
				_caster.RayCast(_head.transform.position, right, num);
				if (_caster.results.Count > 0 && (_caster.results[0].collider.gameObject.layer != 19 || traveled > _minDistanceForPlatform))
				{
					_head.transform.position = _caster.results[0].point;
					yield return _weapon.owner.chronometer.animation.WaitForSeconds(_flyDelay);
					this.onTerrainHit?.Invoke();
					yield break;
				}
				_head.transform.Translate(right * num);
			}
			yield return _weapon.owner.chronometer.animation.WaitForSeconds(_pullDelay);
			this.onExpired?.Invoke();
		}

		public void Pull()
		{
			StopCoroutine("CPull");
			StartCoroutine("CPull");
		}

		private IEnumerator CPull()
		{
			_origin = _pullOrigin;
			Vector3 headPosition = _head.transform.position;
			float time = 0f;
			while (time < 1f)
			{
				yield return null;
				time += _weapon.owner.chronometer.animation.deltaTime * _pullSpeed;
				_head.transform.position = Vector2.LerpUnclamped(headPosition, _origin.transform.position, time);
			}
			_chain.gameObject.SetActive(false);
			_head.gameObject.SetActive(false);
			this.onPullEnd?.Invoke();
		}

		public void Fly()
		{
			StopCoroutine("CFly");
			StartCoroutine("CFly");
		}

		private IEnumerator CFly()
		{
			_origin = _flyOrigin;
			_weapon.owner.movement.configs.Add(2147483646, _flyMovmentConfig);
			float time = 0f;
			while (time < _flyTimeout)
			{
				yield return new WaitForEndOfFrame();
				float deltaTime = _weapon.owner.chronometer.animation.deltaTime;
				time += deltaTime;
				_caster.contactFilter.SetLayerMask(Layers.terrainMask);
				float num = _flySpeed * deltaTime;
				Vector3 vector = _head.transform.position - _weapon.hitbox.bounds.center;
				vector.Normalize();
				_caster.ColliderCast(_weapon.hitbox, vector, num);
				Vector3 vector2 = vector * num;
				Vector3 vector3 = _weapon.owner.transform.position + vector2;
				if ((vector.x > 0f && vector3.x - _head.transform.position.x > 0f) || (vector.x < 0f && vector3.x - _head.transform.position.x < 0f) || _caster.results.Count > 0)
				{
					_head.transform.position = _caster.results[0].point;
					PersistentSingleton<SoundManager>.Instance.PlaySound(_flyEndSound, base.transform.position);
					_weapon.owner.movement.configs.Remove(_flyMovmentConfig);
					_weapon.owner.movement.verticalVelocity = _flyEndVerticalVelocity;
					_chain.gameObject.SetActive(false);
					_head.gameObject.SetActive(false);
					this.onFlyEnd?.Invoke();
					yield break;
				}
				_weapon.owner.movement.force = vector2;
			}
			PersistentSingleton<SoundManager>.Instance.PlaySound(_flyEndSound, base.transform.position);
			_weapon.owner.movement.configs.Remove(_flyMovmentConfig);
			_chain.gameObject.SetActive(false);
			_head.gameObject.SetActive(false);
			this.onFlyEnd?.Invoke();
		}
	}
}
