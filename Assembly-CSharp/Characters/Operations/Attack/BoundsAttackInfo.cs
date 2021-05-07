using Characters.Movements;
using Characters.Operations.Movement;
using FX.BoundsAttackVisualEffect;
using UnityEditor;
using UnityEngine;

namespace Characters.Operations.Attack
{
	public class BoundsAttackInfo : MonoBehaviour
	{
		[SerializeField]
		private HitInfo _hitInfo = new HitInfo(Damage.AttackType.Melee);

		[SerializeField]
		private ChronoInfo _chronoToGlobe;

		[SerializeField]
		private ChronoInfo _chronoToOwner;

		[SerializeField]
		private ChronoInfo _chronoToTarget;

		[SerializeField]
		[Subcomponent(typeof(OperationInfo))]
		private OperationInfo.Subcomponents _operationsToOwner;

		[SerializeField]
		[Subcomponent(typeof(TargetedOperationInfo))]
		private TargetedOperationInfo.Subcomponents _operationInfo;

		[SerializeField]
		[BoundsAttackVisualEffect.Subcomponent]
		private BoundsAttackVisualEffect.Subcomponents _effect;

		internal HitInfo hitInfo => _hitInfo;

		internal OperationInfo.Subcomponents operationsToOwner => _operationsToOwner;

		internal TargetedOperationInfo.Subcomponents operationInfo => _operationInfo;

		internal BoundsAttackVisualEffect.Subcomponents effect => _effect;

		internal PushInfo pushInfo { get; private set; }

		internal void Initialize()
		{
			_operationsToOwner.Initialize();
			_operationInfo.Initialize();
			TargetedOperationInfo[] components = _operationInfo.components;
			foreach (TargetedOperationInfo targetedOperationInfo in components)
			{
				Knockback knockback;
				if ((object)(knockback = targetedOperationInfo.operation as Knockback) != null)
				{
					pushInfo = knockback.pushInfo;
					break;
				}
				Smash smash;
				if ((object)(smash = targetedOperationInfo.operation as Smash) != null)
				{
					pushInfo = smash.pushInfo;
				}
			}
		}

		internal void ApplyChrono(Character owner)
		{
			_chronoToGlobe.ApplyGlobe();
			_chronoToOwner.ApplyTo(owner);
		}

		internal void ApplyChrono(Character owner, Character target)
		{
			_chronoToGlobe.ApplyGlobe();
			_chronoToOwner.ApplyTo(owner);
			_chronoToTarget.ApplyTo(target);
		}
	}
}
