using System.Collections;
using System.Collections.Generic;
using Characters.Abilities.Constraints;
using Characters.Operations;
using PhysicsUtils;
using Services;
using Singletons;
using UnityEngine;

namespace Characters.Gear.Synergy.Keywords.FairyTaleSummon
{
	public class Spirit : MonoBehaviour
	{
		private Character _owner;

		[SerializeField]
		[Constraint.Subcomponent]
		private Constraint.Subcomponents _constraints;

		[SerializeField]
		private Transform _slot;

		[SerializeField]
		private float _trackSpeed = 2.5f;

		[Space]
		[SerializeField]
		private Animator _animator;

		[Space]
		[SerializeField]
		private Collider2D _detectRange;

		[Space]
		[Tooltip("오퍼레이션 프리팹")]
		[SerializeField]
		private OperationRunner _operationRunner;

		private TargetLayer _layer = new TargetLayer(0, false, true, false, false);

		private NonAllocOverlapper _overlapper = new NonAllocOverlapper(1);

		private float _attackCooldown;

		private void Awake()
		{
			_detectRange.enabled = false;
		}

		private void OnEnable()
		{
			StartCoroutine(CRun());
			ResetPosition();
			Singleton<Service>.Instance.levelManager.onMapLoaded += ResetPosition;
		}

		private void OnDisable()
		{
			Singleton<Service>.Instance.levelManager.onMapLoaded -= ResetPosition;
		}

		private void ResetPosition()
		{
			base.transform.position = _slot.transform.position;
		}

		public void Initialize(Character owner)
		{
			_owner = owner;
		}

		public void Set(int attackCooldown, RuntimeAnimatorController graphic)
		{
			_attackCooldown = attackCooldown;
			_animator.runtimeAnimatorController = graphic;
		}

		private bool FindTarget(out Target target)
		{
			_overlapper.contactFilter.SetLayerMask(_layer.Evaluate(_owner.gameObject));
			_detectRange.enabled = true;
			_overlapper.OverlapCollider(_detectRange);
			_detectRange.enabled = false;
			List<Target> components = _overlapper.results.GetComponents<Collider2D, Target>();
			if (components.Count == 0)
			{
				target = null;
				return false;
			}
			target = components[0];
			return true;
		}

		private void Move(float deltaTime)
		{
			base.transform.position = Vector3.Lerp(base.transform.position, _slot.transform.position, deltaTime * _trackSpeed);
		}

		private IEnumerator CRun()
		{
			float time = 0f;
			while (true)
			{
				Target target;
				if (time < _attackCooldown)
				{
					yield return null;
					float deltaTime = _owner.chronometer.master.deltaTime;
					time += deltaTime;
					Move(deltaTime);
				}
				else if (!_constraints.components.Pass())
				{
					time -= _owner.chronometer.master.deltaTime;
					yield return null;
				}
				else if (!FindTarget(out target))
				{
					time = _attackCooldown - 0.25f;
				}
				else
				{
					time = 0f;
					yield return CSpawnOperationRunner(target);
				}
			}
		}

		private IEnumerator CSpawnOperationRunner(Target target)
		{
			Vector3 vector = target.collider.bounds.center - base.transform.position;
			float z = Mathf.Atan2(vector.y, vector.x) * 57.29578f;
			OperationInfos spawnedOperationInfos = _operationRunner.Spawn().operationInfos;
			spawnedOperationInfos.transform.SetPositionAndRotation(base.transform.position, Quaternion.Euler(0f, 0f, z));
			spawnedOperationInfos.Run(_owner);
			while (spawnedOperationInfos.gameObject.activeSelf)
			{
				yield return null;
			}
		}
	}
}
