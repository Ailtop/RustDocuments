using UnityEditor;
using UnityEngine;

namespace Characters.Actions
{
	public class ParryAction : Action
	{
		[SerializeField]
		private bool _ignoreDamage;

		[SerializeField]
		private bool _frontOnly = true;

		[SerializeField]
		[MinMaxSlider(0f, 1f)]
		private Vector2 _countableRange = new Vector2(0f, 1f);

		[SerializeField]
		[Subcomponent(typeof(Motion))]
		protected Motion _waitingMotion;

		[SerializeField]
		[Subcomponent(typeof(Motion))]
		protected Motion _parryMotion;

		private Character.LookingDirection _lookingDirection;

		public Motion waitingMotion => _waitingMotion;

		public Motion parryMotion => _parryMotion;

		public override Motion[] motions => new Motion[2] { _waitingMotion, _parryMotion };

		public override bool canUse
		{
			get
			{
				if (base.cooldown.canUse && !_owner.stunedOrFreezed)
				{
					return PassAllConstraints(_waitingMotion);
				}
				return false;
			}
		}

		protected override void Awake()
		{
			base.Awake();
			_waitingMotion.onStart += delegate
			{
				_lookingDirection = base.owner.lookingDirection;
				_owner.health.onTakeDamage.Add(int.MinValue, OnTakeDamage);
			};
			_waitingMotion.onEnd += delegate
			{
				_owner.health.onTakeDamage.Remove(OnTakeDamage);
			};
			_waitingMotion.onCancel += delegate
			{
				_owner.health.onTakeDamage.Remove(OnTakeDamage);
			};
			_parryMotion.onStart += delegate
			{
				base.owner.lookingDirection = _lookingDirection;
			};
			if (_countableRange.y == 1f)
			{
				_countableRange.y = float.PositiveInfinity;
			}
		}

		public override void Initialize(Character owner)
		{
			base.Initialize(owner);
			_waitingMotion.Initialize(this);
			_parryMotion.Initialize(this);
		}

		private void OnDisable()
		{
			_owner.health.onTakeDamage.Remove(OnTakeDamage);
		}

		private bool OnTakeDamage(ref Damage damage)
		{
			if (damage.attackType == Damage.AttackType.Additional || !MMMaths.Range(_waitingMotion.normalizedTime, _countableRange))
			{
				return false;
			}
			Vector3 position = base.transform.position;
			Vector3 position2 = damage.attacker.transform.position;
			if (!_frontOnly || (_owner.lookingDirection == Character.LookingDirection.Right && position.x < position2.x) || (_owner.lookingDirection == Character.LookingDirection.Left && position.x > position2.x))
			{
				DoMotion(_parryMotion);
				return _ignoreDamage;
			}
			return false;
		}

		public override bool TryStart()
		{
			if (!canUse || !ConsumeCooldownIfNeeded())
			{
				return false;
			}
			DoAction(_waitingMotion);
			return true;
		}
	}
}
