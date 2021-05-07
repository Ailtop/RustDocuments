using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Characters.Operations;
using UnityEngine;

namespace Characters.Gear.Synergy.Keywords
{
	public class Swamp : Keyword, IAttackDamage
	{
		[SerializeField]
		private int _poisonPossibility = 10;

		[Space]
		[Information("Poison의 Damage는 초당 피해량을 의미합니다. Poison은 초당 2번의 피해를 입힙니다.", InformationAttribute.InformationType.Info, false)]
		[SerializeField]
		private CharacterStatus.ApplyInfo _statusInfoByBasicAttack;

		[Space]
		[SerializeField]
		private Transform _range;

		[Space]
		[SerializeField]
		private int[] _conversionPercentByLevel;

		[Space]
		[SerializeField]
		private float _explosionDelay;

		private WaitForSeconds _waitForExplosionDelay;

		[Space]
		[SerializeField]
		[CharacterOperation.Subcomponent]
		private CharacterOperation.Subcomponents _operation;

		public override Key key => Key.Swamp;

		protected override IList valuesByLevel => _conversionPercentByLevel;

		public float amount { get; private set; }

		private void Awake()
		{
			_waitForExplosionDelay = new WaitForSeconds(_explosionDelay);
		}

		protected override void Initialize()
		{
			_operation.Initialize();
		}

		protected override void UpdateBonus()
		{
		}

		protected override void OnAttach()
		{
			Character obj = base.character;
			obj.onGaveDamage = (GaveDamageDelegate)Delegate.Combine(obj.onGaveDamage, new GaveDamageDelegate(OnGaveDamage));
			Character obj2 = base.character;
			obj2.onKilled = (Character.OnKilledDelegate)Delegate.Combine(obj2.onKilled, new Character.OnKilledDelegate(OnKilled));
		}

		protected override void OnDetach()
		{
			Character obj = base.character;
			obj.onGaveDamage = (GaveDamageDelegate)Delegate.Remove(obj.onGaveDamage, new GaveDamageDelegate(OnGaveDamage));
			Character obj2 = base.character;
			obj2.onKilled = (Character.OnKilledDelegate)Delegate.Remove(obj2.onKilled, new Character.OnKilledDelegate(OnKilled));
		}

		private void OnGaveDamage(ITarget target, [In][IsReadOnly] ref Damage originalDamage, [In][IsReadOnly] ref Damage tookDamage, double damageDealt)
		{
			if (!(target.character == null) && !(target.character.status == null))
			{
				Damage damage = tookDamage;
				if (!(damage.amount < 1.0) && tookDamage.attackType != 0 && tookDamage.motionType == Damage.MotionType.Basic && MMMaths.PercentChance(_poisonPossibility))
				{
					base.character.GiveStatus(target.character, _statusInfoByBasicAttack);
				}
			}
		}

		private void OnKilled(ITarget target, ref Damage damage)
		{
			if (target != null && !(target.character == null) && !(target.character.status == null) && target.character.status.poisoned)
			{
				amount = (float)(target.character.status.poison.GetRemainDamage() * (double)_conversionPercentByLevel[base.level] * 0.01);
				if (!(amount < 1f))
				{
					Vector3 position = target.transform.position;
					Vector2 offset = target.collider.offset;
					position.x += offset.x;
					position.y += offset.y;
					StartCoroutine(CSwampExplode(position, amount));
				}
			}
		}

		private IEnumerator CSwampExplode(Vector3 position, double damageAmount)
		{
			yield return _waitForExplosionDelay;
			_range.transform.position = position;
			_operation.Run(base.character);
		}
	}
}
