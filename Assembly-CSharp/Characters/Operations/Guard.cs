using System.Collections;
using UnityEditor;
using UnityEngine;

namespace Characters.Operations
{
	public class Guard : CharacterOperation
	{
		[SerializeField]
		private bool _ignoreDamage;

		[SerializeField]
		private bool _frontOnly = true;

		[SerializeField]
		[Subcomponent]
		private Subcomponents _onHitToOwner;

		[SerializeField]
		private ChronoInfo _onHitToOwnerChronoInfo;

		[SerializeField]
		[Subcomponent]
		private Subcomponents _onHitToOwnerFromRangeAttack;

		[SerializeField]
		[Subcomponent]
		private Subcomponents _onHitToTarget;

		[SerializeField]
		private ChronoInfo _onHitToTargetChronoInfo;

		[SerializeField]
		private float _duration;

		[Header("Break")]
		[SerializeField]
		private bool _breakable;

		[SerializeField]
		private float _breakDamage;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onBreak;

		private Character _owner;

		private Coroutine _cExpire;

		[SerializeField]
		private float _chronoEffectUniqueTime = 0.2f;

		private float _lastTime;

		private void Awake()
		{
			if (_onBreak != null)
			{
				_onBreak.Initialize();
			}
			_lastTime = Time.time;
		}

		private bool Block(ref Damage damage)
		{
			Attacker attacker = damage.attacker;
			if (damage.attackType == Damage.AttackType.Additional)
			{
				return false;
			}
			Vector3 position = base.transform.position;
			Vector3 position2 = damage.attacker.transform.position;
			if (!_frontOnly || (_owner.lookingDirection == Character.LookingDirection.Right && position.x < position2.x) || (_owner.lookingDirection == Character.LookingDirection.Left && position.x > position2.x))
			{
				if (_breakable && damage.amount >= (double)_breakDamage)
				{
					damage.stoppingPower = 2f;
					_onBreak.gameObject.SetActive(true);
					_onBreak.Run(_owner);
					Stop();
					return true;
				}
				damage.stoppingPower = 0f;
				if (damage.attackType == Damage.AttackType.Melee)
				{
					if (Time.time - _lastTime < _chronoEffectUniqueTime)
					{
						return true;
					}
					_lastTime = Time.time;
					_onHitToOwnerChronoInfo.ApplyGlobe();
					if (_onHitToOwner.components.Length != 0)
					{
						for (int i = 0; i < _onHitToOwner.components.Length; i++)
						{
							_onHitToOwner.components[i].Run(_owner);
						}
					}
					if (_onHitToTarget.components.Length != 0)
					{
						for (int j = 0; j < _onHitToTarget.components.Length; j++)
						{
							_onHitToTarget.components[j].Run(attacker.character);
						}
					}
				}
				else if ((damage.attackType == Damage.AttackType.Ranged || damage.attackType == Damage.AttackType.Projectile) && _onHitToOwnerFromRangeAttack.components.Length != 0)
				{
					for (int k = 0; k < _onHitToOwnerFromRangeAttack.components.Length; k++)
					{
						_onHitToOwnerFromRangeAttack.components[k].Run(_owner);
					}
				}
				return true;
			}
			_owner.CancelAction();
			return false;
		}

		public override void Run(Character owner)
		{
			_owner = owner;
			_owner.health.onTakeDamage.Add(int.MinValue, Block);
			if (_duration > 0f)
			{
				_cExpire = StartCoroutine(CExpire());
			}
		}

		private IEnumerator CExpire()
		{
			yield return _owner.chronometer.master.WaitForSeconds(_duration);
			Stop();
		}

		public override void Stop()
		{
			if (_cExpire != null)
			{
				StopCoroutine(_cExpire);
			}
			_owner?.health.onTakeDamage.Remove(Block);
		}
	}
}
