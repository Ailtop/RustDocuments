using Characters.Operations.Attack;
using UnityEditor;
using UnityEngine;

namespace Characters.Actions
{
	public class AttackHitTriggerAction : Action
	{
		[SerializeField]
		[Subcomponent(typeof(Motion))]
		protected Motion _attackMotion;

		[SerializeField]
		[Subcomponent(typeof(Motion))]
		protected Motion _secondMotion;

		private IAttack _attack;

		public override bool canUse
		{
			get
			{
				if (base.cooldown.canUse && !_owner.stunedOrFreezed)
				{
					return PassAllConstraints(_attackMotion);
				}
				return false;
			}
		}

		public override Motion[] motions => new Motion[2] { _attackMotion, _secondMotion };

		protected override void Awake()
		{
			base.Awake();
			_attack = _attackMotion.GetComponentInChildren<IAttack>();
			if (_attack == null)
			{
				Debug.LogError("Attack is null " + base.gameObject.name);
			}
			else
			{
				_attack.onHit += OnAttackHit;
			}
		}

		public override void Initialize(Character owner)
		{
			base.Initialize(owner);
			_attackMotion.Initialize(this);
			_secondMotion.Initialize(this);
		}

		private void OnDestroy()
		{
			_attack.onHit -= OnAttackHit;
		}

		public override bool TryStart()
		{
			if (!canUse || !ConsumeCooldownIfNeeded())
			{
				return false;
			}
			DoAction(_attackMotion);
			return true;
		}

		private void OnAttackHit(Target target, ref Damage damage)
		{
			_attackMotion.EndBehaviour();
			DoMotion(_secondMotion);
		}
	}
}
