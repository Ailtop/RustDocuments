using System.Collections.Generic;
using FX.BoundsAttackVisualEffect;
using PhysicsUtils;
using UnityEngine;

namespace Characters.Operations
{
	public class ShadeInstantAttack : CharacterOperation
	{
		private const int _limit = 3;

		[SerializeField]
		private HitInfo _hitInfo = new HitInfo(Damage.AttackType.Melee);

		[SerializeField]
		private Collider2D _attackRange;

		[SerializeField]
		private int _damage1;

		[SerializeField]
		private int _damage2;

		[SerializeField]
		private int _damage3;

		[SerializeField]
		[BoundsAttackVisualEffect.Subcomponent]
		private BoundsAttackVisualEffect.Subcomponents _effect1;

		[SerializeField]
		[BoundsAttackVisualEffect.Subcomponent]
		private BoundsAttackVisualEffect.Subcomponents _effect2;

		[SerializeField]
		[BoundsAttackVisualEffect.Subcomponent]
		private BoundsAttackVisualEffect.Subcomponents _effect3;

		private int[] _damages;

		private BoundsAttackVisualEffect.Subcomponents[] _effects;

		private TargetLayer _layer = new TargetLayer(0, false, true, false, false);

		private NonAllocOverlapper _overlapper;

		private float _remainTimeToNextAttack;

		private void Awake()
		{
			_overlapper = new NonAllocOverlapper(3);
			_damages = new int[3] { _damage1, _damage2, _damage3 };
			_effects = new BoundsAttackVisualEffect.Subcomponents[3] { _effect1, _effect2, _effect3 };
			_attackRange.enabled = false;
		}

		public override void Run(Character owner)
		{
			_overlapper.contactFilter.SetLayerMask(_layer.Evaluate(owner.gameObject));
			_attackRange.enabled = true;
			Bounds bounds = _attackRange.bounds;
			_overlapper.OverlapCollider(_attackRange);
			_attackRange.enabled = false;
			List<Target> components = _overlapper.results.GetComponents<Collider2D, Target>();
			if (components.Count == 0)
			{
				return;
			}
			int num = _damages[components.Count - 1];
			BoundsAttackVisualEffect.Subcomponents subcomponents = _effects[components.Count - 1];
			for (int i = 0; i < components.Count; i++)
			{
				Target target = components[i];
				if (!(target == null) && !(target.character == null) && !(target.character == owner) && target.character.liveAndActive)
				{
					Bounds bounds2 = target.collider.bounds;
					Bounds bounds3 = default(Bounds);
					bounds3.min = MMMaths.Max(bounds.min, bounds2.min);
					bounds3.max = MMMaths.Min(bounds.max, bounds2.max);
					Vector2 hitPoint = MMMaths.RandomPointWithinBounds(bounds3);
					Damage damage = owner.stat.GetDamage(num, hitPoint, _hitInfo);
					subcomponents.Spawn(owner, bounds3, ref damage, target);
					if (!target.character.invulnerable.value)
					{
						owner.AttackCharacter(target, ref damage);
					}
				}
			}
		}
	}
}
