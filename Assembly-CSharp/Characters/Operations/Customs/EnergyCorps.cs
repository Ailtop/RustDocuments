using System;
using System.Collections;
using Characters.AI;
using Characters.Projectiles;
using Characters.Utils;
using UnityEngine;

namespace Characters.Operations.Customs
{
	public class EnergyCorps : CharacterOperation
	{
		[Serializable]
		private class FireEnergyCrops
		{
			internal enum DirectionType
			{
				RotationOfFirePosition,
				OwnerDirection,
				Constant
			}

			[SerializeField]
			internal Projectile projectile;

			[SerializeField]
			internal bool group;

			[SerializeField]
			internal bool platformTarget;

			[SerializeField]
			internal DirectionType directionType;

			[SerializeField]
			internal CustomAngle.Reorderable directions;
		}

		[SerializeField]
		private float _interval;

		[SerializeField]
		private AIController _controller;

		[SerializeField]
		private Transform _energyCorpsContainer;

		[Header("Fire Projectile")]
		[SerializeField]
		private FireEnergyCrops _fireEnergyCrops;

		private HitHistoryManager _hitHistoryManager;

		private IAttackDamage _attackDamage;

		private Coroutine _cReference;

		public override void Initialize()
		{
			base.Initialize();
			_attackDamage = GetComponentInParent<IAttackDamage>();
		}

		public override void Run(Character owner)
		{
			_hitHistoryManager = (_fireEnergyCrops.group ? new HitHistoryManager(15) : null);
			_cReference = StartCoroutine(CRun(owner));
		}

		private IEnumerator CRun(Character owner)
		{
			Character target = _controller.target;
			foreach (Transform item in _energyCorpsContainer)
			{
				Vector3 vector = new Vector3(y: (!_fireEnergyCrops.platformTarget) ? (target.transform.position.y + target.collider.bounds.extents.y) : target.movement.controller.collisionState.lastStandingCollider.bounds.max.y, x: target.transform.position.x) - item.transform.position;
				float z = Mathf.Atan2(vector.y, vector.x) * 57.29578f;
				item.rotation = Quaternion.Euler(0f, 0f, z);
				item.gameObject.SetActive(false);
				FireProjectile(owner, item);
				yield return owner.chronometer.master.WaitForSeconds(_interval);
			}
		}

		private void FireProjectile(Character owner, Transform firePosition)
		{
			CustomAngle[] values = _fireEnergyCrops.directions.values;
			if (_fireEnergyCrops.directionType == FireEnergyCrops.DirectionType.RotationOfFirePosition)
			{
				for (int i = 0; i < values.Length; i++)
				{
					_fireEnergyCrops.projectile.reusable.Spawn(firePosition.position).GetComponent<Projectile>().Fire(owner, _attackDamage.amount, firePosition.localRotation.eulerAngles.z + values[i].value, firePosition.lossyScale.x < 0f);
				}
			}
			else if (_fireEnergyCrops.directionType == FireEnergyCrops.DirectionType.OwnerDirection)
			{
				for (int j = 0; j < values.Length; j++)
				{
					_fireEnergyCrops.projectile.reusable.Spawn(firePosition.position).GetComponent<Projectile>().Fire(owner, _attackDamage.amount, values[j].value, owner.lookingDirection == Character.LookingDirection.Left);
				}
			}
			else
			{
				for (int k = 0; k < values.Length; k++)
				{
					_fireEnergyCrops.projectile.reusable.Spawn(firePosition.position).GetComponent<Projectile>().Fire(owner, _attackDamage.amount, values[k].value, false, false, 1f, _fireEnergyCrops.group ? _hitHistoryManager : null);
				}
			}
		}

		public override void Stop()
		{
			base.Stop();
			if (_cReference != null)
			{
				StopCoroutine(_cReference);
			}
		}
	}
}
