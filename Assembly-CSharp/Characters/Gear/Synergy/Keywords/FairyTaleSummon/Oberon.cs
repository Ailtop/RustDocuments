using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Characters.Abilities.Constraints;
using Characters.Operations;
using Characters.Operations.Fx;
using FX;
using PhysicsUtils;
using Services;
using Singletons;
using UnityEngine;

namespace Characters.Gear.Synergy.Keywords.FairyTaleSummon
{
	public class Oberon : MonoBehaviour
	{
		private const string _attackName = "Attack";

		private static readonly int _attackHash = Animator.StringToHash("Attack");

		private float _attackLength;

		private const string _idleName = "Idle";

		private static readonly int _idleHash = Animator.StringToHash("Idle");

		private float _idleLength;

		private const string _introName = "Intro";

		private static readonly int _introHash = Animator.StringToHash("Intro");

		private float _introLength;

		private const string _bombEndName = "SpiritBomb_End";

		private static readonly int _bombEndHash = Animator.StringToHash("SpiritBomb_End");

		private float _bombEndLength;

		private const string _bombLoopName = "SpiritBomb_Loop";

		private static readonly int _bombLoopHash = Animator.StringToHash("SpiritBomb_Loop");

		private float _bombLoopLength;

		private const string _bombReadyName = "SpiritBomb_Ready";

		private static readonly int _bombReadyHash = Animator.StringToHash("SpiritBomb_Ready");

		private float _bombReadyLength;

		private const string _thunderName = "SpiritThunder";

		private static readonly int _thunderHash = Animator.StringToHash("SpiritThunder");

		private float _thunderLength;

		private Character _owner;

		[SerializeField]
		[Constraint.Subcomponent]
		private Constraint.Subcomponents _constraints;

		[Header("Movement")]
		[SerializeField]
		private Transform _slot;

		[SerializeField]
		private float _trackSpeed = 2.5f;

		[SerializeField]
		private float _floatAmplitude = 0.5f;

		[SerializeField]
		private float _floatFrequency = 1f;

		[Header("Graphic")]
		[SerializeField]
		private Animator _animator;

		[SerializeField]
		private SpriteRenderer _spriteRenderer;

		[SerializeField]
		private EffectInfo _introEffect;

		[Header("Galaxy Beam")]
		[SerializeField]
		private Collider2D _attackDetectRange;

		[SerializeField]
		private EffectInfo _attackEffect;

		[SerializeField]
		private OperationRunner _attackOperationRunner;

		[SerializeField]
		private float _attackCooldown;

		private float _remainAttackCooldown;

		[Header("Spirit Thunder")]
		[SerializeField]
		private Collider2D _thunderDetectRange;

		[SerializeField]
		private EffectInfo _thunderEffect;

		[SerializeField]
		private OperationRunner _thunderOperationRunner;

		[SerializeField]
		private float _thunderCooldown;

		private float _remainThunderCooldown;

		[Header("Spirit Nemesis")]
		[SerializeField]
		private Collider2D _bombDetectRange;

		[SerializeField]
		private EffectInfo _bombEffect;

		[SerializeField]
		private Characters.Operations.Fx.ScreenFlash _bombScreenFlash;

		[SerializeField]
		private OperationRunner _bombOperationRunner;

		[SerializeField]
		private float _bombCooldown;

		[SerializeField]
		private Transform _bombSpawnPosition;

		private float _remainBombCooldown;

		private TargetLayer _layer = new TargetLayer(0, false, true, false, false);

		private NonAllocOverlapper _overlapper = new NonAllocOverlapper(1);

		private RayCaster _groundFinder;

		private Vector3 _position;

		private float _floatingTime;

		private void Awake()
		{
			_attackDetectRange.enabled = false;
			_thunderDetectRange.enabled = false;
			_bombDetectRange.enabled = false;
			_groundFinder = new RayCaster
			{
				direction = Vector2.down,
				distance = 5f
			};
			_groundFinder.contactFilter.SetLayerMask(Layers.groundMask);
			_remainAttackCooldown = _attackCooldown;
			_remainThunderCooldown = _thunderCooldown;
			_remainBombCooldown = _bombCooldown;
			_bombScreenFlash.Initialize();
			Dictionary<string, AnimationClip> dictionary = _animator.runtimeAnimatorController.animationClips.ToDictionary((AnimationClip clip) => clip.name);
			_idleLength = dictionary["Idle"].length;
			_introLength = dictionary["Intro"].length;
			_bombEndLength = dictionary["SpiritBomb_End"].length;
			_bombLoopLength = dictionary["SpiritBomb_Loop"].length;
			_bombReadyLength = dictionary["SpiritBomb_Ready"].length;
			_thunderLength = dictionary["SpiritThunder"].length;
		}

		public void Initialize(Character owner)
		{
			_owner = owner;
		}

		private void OnEnable()
		{
			ResetPosition();
			StartCoroutine(CCooldown());
			StartCoroutine(CRun());
			Singleton<Service>.Instance.levelManager.onMapLoaded += ResetPosition;
		}

		private void OnDisable()
		{
			Singleton<Service>.Instance.levelManager.onMapLoaded -= ResetPosition;
		}

		private void ResetPosition()
		{
			base.transform.position = (_position = _slot.transform.position);
		}

		private void Move(float deltaTime)
		{
			_position = Vector3.Lerp(_position, _slot.transform.position, deltaTime * _trackSpeed);
			_floatingTime += deltaTime;
			Vector3 position = _position;
			position.y += Mathf.Sin(_floatingTime * (float)Math.PI * _floatFrequency) * _floatAmplitude;
			base.transform.position = position;
			_spriteRenderer.flipX = _slot.transform.position.x - _position.x < 0f;
		}

		private IEnumerator CCooldown()
		{
			while (true)
			{
				yield return null;
				if (_constraints.components.Pass())
				{
					_remainAttackCooldown -= _owner.chronometer.master.deltaTime;
					_remainThunderCooldown -= _owner.chronometer.master.deltaTime;
					_remainBombCooldown -= _owner.chronometer.master.deltaTime;
				}
			}
		}

		private IEnumerator CPlayAnimation(int hash, float length)
		{
			_animator.Play(hash);
			_animator.enabled = false;
			float remain = length;
			while (remain > float.Epsilon)
			{
				float deltaTime = Chronometer.global.deltaTime;
				_animator.Update(deltaTime);
				remain -= deltaTime;
				yield return null;
			}
			_animator.enabled = true;
		}

		private IEnumerator CRun()
		{
			_introEffect.Spawn(base.transform.position);
			yield return CPlayAnimation(_introHash, _introLength);
			while (true)
			{
				_animator.Play(_idleHash);
				yield return null;
				Move(_owner.chronometer.master.deltaTime);
				if (_remainAttackCooldown < 0f)
				{
					Target target;
					if (FindAttackTarget(out target, _attackDetectRange))
					{
						_remainAttackCooldown = _attackCooldown;
						_animator.Play(_attackHash);
						_attackEffect.Spawn(base.transform.position);
						yield return CSpawnAttackOperationRunner(target);
					}
					else
					{
						_remainAttackCooldown = 0.5f;
					}
				}
				if (_remainThunderCooldown < 0f)
				{
					Vector3 position;
					if (FindThunderPosition(out position))
					{
						_remainThunderCooldown = _thunderCooldown;
						_animator.Play(_thunderHash);
						_thunderEffect.Spawn(base.transform.position);
						yield return CSpawnThunderOperationRunner(position);
					}
					else
					{
						_remainThunderCooldown = 0.5f;
					}
				}
				if (_remainBombCooldown < 0f)
				{
					Target target2;
					if (FindAttackTarget(out target2, _bombDetectRange))
					{
						_remainBombCooldown = _bombCooldown;
						yield return CPlayAnimation(_bombReadyHash, _bombReadyLength);
						_animator.Play(_bombLoopHash);
						_bombEffect.Spawn(base.transform.position);
						_bombScreenFlash.Run(_owner);
						yield return CSpawnBombOperationRunner();
						yield return CPlayAnimation(_bombEndHash, _bombEndLength);
					}
					else
					{
						_remainBombCooldown = 0.5f;
					}
				}
			}
		}

		private bool FindAttackTarget(out Target target, Collider2D collider)
		{
			_overlapper.contactFilter.SetLayerMask(_layer.Evaluate(_owner.gameObject));
			collider.enabled = true;
			_overlapper.OverlapCollider(collider);
			collider.enabled = false;
			List<Target> components = _overlapper.results.GetComponents<Collider2D, Target>();
			if (components.Count == 0)
			{
				target = null;
				return false;
			}
			target = components[0];
			return true;
		}

		private bool FindThunderPosition(out Vector3 position)
		{
			position = Vector3.zero;
			_overlapper.contactFilter.SetLayerMask(_layer.Evaluate(_owner.gameObject));
			_thunderDetectRange.enabled = true;
			_overlapper.OverlapCollider(_thunderDetectRange);
			_thunderDetectRange.enabled = false;
			List<Target> components = _overlapper.results.GetComponents<Collider2D, Target>();
			if (components.Count == 0)
			{
				return false;
			}
			Target target = components[0];
			_groundFinder.origin = target.transform.position;
			RaycastHit2D raycastHit2D = _groundFinder.SingleCast();
			if (!raycastHit2D)
			{
				return false;
			}
			position = raycastHit2D.point;
			return true;
		}

		private IEnumerator CSpawnAttackOperationRunner(Target target)
		{
			Vector3 vector = target.collider.bounds.center - base.transform.position;
			float z = Mathf.Atan2(vector.y, vector.x) * 57.29578f;
			OperationInfos spawnedOperationInfos = _attackOperationRunner.Spawn().operationInfos;
			spawnedOperationInfos.transform.SetPositionAndRotation(base.transform.position, Quaternion.Euler(0f, 0f, z));
			spawnedOperationInfos.Run(_owner);
			while (spawnedOperationInfos.gameObject.activeSelf)
			{
				yield return null;
			}
		}

		private IEnumerator CSpawnThunderOperationRunner(Vector3 position)
		{
			OperationInfos spawnedOperationInfos = _thunderOperationRunner.Spawn().operationInfos;
			spawnedOperationInfos.transform.SetPositionAndRotation(position, Quaternion.identity);
			spawnedOperationInfos.Run(_owner);
			while (spawnedOperationInfos.gameObject.activeSelf)
			{
				yield return null;
			}
		}

		private IEnumerator CSpawnBombOperationRunner()
		{
			OperationInfos spawnedOperationInfos = _bombOperationRunner.Spawn().operationInfos;
			spawnedOperationInfos.transform.SetPositionAndRotation(_bombSpawnPosition.position, Quaternion.identity);
			spawnedOperationInfos.Run(_owner);
			while (spawnedOperationInfos.gameObject.activeSelf)
			{
				yield return null;
			}
		}
	}
}
