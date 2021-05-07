using Characters.Movements;
using FX.SmashAttackVisualEffect;
using UnityEditor;
using UnityEngine;

namespace Characters.Operations.Movement
{
	public class SmashTo : TargetedCharacterOperation
	{
		[Header("Destination")]
		[SerializeField]
		private Collider2D _targetPlace;

		[SerializeField]
		private Transform _targetPoint;

		[Header("Force")]
		[SerializeField]
		private Curve _curve;

		[SerializeField]
		private bool _ignoreOtherForce = true;

		[SerializeField]
		private bool _expireOnGround;

		[Header("Hit")]
		[SerializeField]
		private HitInfo _hitInfo = new HitInfo(Damage.AttackType.Additional);

		[SerializeField]
		[SmashAttackVisualEffect.Subcomponent]
		private SmashAttackVisualEffect.Subcomponents _effect;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(TargetedOperationInfo))]
		private TargetedOperationInfo.Subcomponents _onCollide;

		private IAttackDamage _attackDamage;

		public override void Initialize()
		{
			_attackDamage = GetComponentInParent<IAttackDamage>();
			_onCollide.Initialize();
		}

		private void OnEnd(Push push, Character from, Character to, Push.SmashEndType endType, RaycastHit2D? raycastHit, Characters.Movements.Movement.CollisionDirection direction)
		{
			if (endType == Push.SmashEndType.Collide)
			{
				StartCoroutine(_onCollide.CRun(from, to));
				Damage damage = from.stat.GetDamage(_attackDamage.amount, raycastHit.Value.point, _hitInfo);
				TargetStruct targetStruct = new TargetStruct(to);
				from.AttackCharacter(targetStruct, ref damage);
				_effect.Spawn(to, push, raycastHit.Value, direction, damage, targetStruct);
			}
		}

		public override void Run(Character owner, Character target)
		{
			Vector2 vector = ((!(_targetPlace != null)) ? ((Vector2)_targetPoint.position) : MMMaths.RandomPointWithinBounds(_targetPlace.bounds));
			Vector2 force = vector - (Vector2)target.transform.position;
			target.movement.push.ApplySmash(owner, force, _curve, _ignoreOtherForce, _expireOnGround, OnEnd);
		}
	}
}
