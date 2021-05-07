using Characters.Movements;
using FX.SmashAttackVisualEffect;
using UnityEditor;
using UnityEngine;

namespace Characters.Operations.Movement
{
	public class Smash : TargetedCharacterOperation
	{
		[SerializeField]
		[Information("이 값을 지정해주면 오퍼레이션 소유 캐릭터 대신 해당 트랜스폼을 기준으로 넉백합니다.", InformationAttribute.InformationType.Info, false)]
		private Transform _transfromOverride;

		[SerializeField]
		private PushInfo _pushInfo = new PushInfo(true);

		[SerializeField]
		private HitInfo _hitInfo = new HitInfo(Damage.AttackType.Additional);

		[SerializeField]
		[SmashAttackVisualEffect.Subcomponent]
		private SmashAttackVisualEffect.Subcomponents _effect;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(TargetedOperationInfo))]
		private TargetedOperationInfo.Subcomponents _onCollide;

		private IAttackDamage _attackDamage;

		public PushInfo pushInfo => _pushInfo;

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
			if (_transfromOverride == null)
			{
				target.movement.push.ApplySmash(owner, _pushInfo, OnEnd);
			}
			else
			{
				target.movement.push.ApplySmash(owner, _transfromOverride, _pushInfo, OnEnd);
			}
		}
	}
}
