using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Characters.Operations;
using FX;
using Singletons;
using UnityEngine;

namespace Characters.Gear.Synergy.Keywords
{
	public class Blizzard : Keyword, IAttackDamage
	{
		[SerializeField]
		private int _freezePossiblity = 5;

		[Space]
		[SerializeField]
		private CharacterStatus.ApplyInfo _statusInfo;

		[Space]
		[SerializeField]
		private Transform _range;

		[Space]
		[Tooltip("타겟의 bottom으로 이동됨")]
		[SerializeField]
		private Transform _effectPosition;

		[Space]
		[SerializeField]
		private float[] _afterShockDamage;

		[Space]
		[SerializeField]
		private SoundInfo _freezeSound;

		[Space]
		[SerializeField]
		[CharacterOperation.Subcomponent]
		private CharacterOperation.Subcomponents _afterShockOperation;

		public override Key key => Key.Blizzard;

		protected override IList valuesByLevel => _afterShockDamage;

		public float amount { get; private set; }

		protected override void Initialize()
		{
			_afterShockOperation.Initialize();
		}

		protected override void UpdateBonus()
		{
			amount = _afterShockDamage[base.level];
		}

		protected override void OnAttach()
		{
			Character obj = base.character;
			obj.onGaveDamage = (GaveDamageDelegate)Delegate.Combine(obj.onGaveDamage, new GaveDamageDelegate(OnGaveDamage));
		}

		protected override void OnDetach()
		{
			Character obj = base.character;
			obj.onGaveDamage = (GaveDamageDelegate)Delegate.Remove(obj.onGaveDamage, new GaveDamageDelegate(OnGaveDamage));
		}

		private void OnGaveDamage(ITarget target, [In][IsReadOnly] ref Damage originalDamage, [In][IsReadOnly] ref Damage tookDamage, double damageDealt)
		{
			if (target.character == null)
			{
				return;
			}
			Damage damage = tookDamage;
			if (damage.amount != 0.0)
			{
				if (tookDamage.motionType == Damage.MotionType.Status && tookDamage.key.Equals("status_freeze"))
				{
					_range.transform.position = target.collider.bounds.center;
					_effectPosition.transform.position = target.transform.position;
					_afterShockOperation.Run(base.character);
				}
				else if (tookDamage.attackType != 0 && tookDamage.motionType == Damage.MotionType.Skill && MMMaths.PercentChance(_freezePossiblity) && !(target.character.status == null))
				{
					PersistentSingleton<SoundManager>.Instance.PlaySound(_freezeSound, target.transform.position);
					target.character.status.Apply(base.character, _statusInfo);
				}
			}
		}
	}
}
