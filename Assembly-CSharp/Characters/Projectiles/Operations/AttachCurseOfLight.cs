using System.Runtime.CompilerServices;
using Characters.Abilities;
using Characters.Abilities.Enemies;
using UnityEngine;

namespace Characters.Projectiles.Operations
{
	public sealed class AttachCurseOfLight : CharacterHitOperation
	{
		[SerializeField]
		[AbilityComponent.Subcomponent]
		private AbilityComponent _abilityComponent;

		private IAbilityInstance _curseCache;

		public override void Run(Projectile projectile, RaycastHit2D raycastHit, Character target)
		{
			_abilityComponent.Initialize();
			if (_curseCache != null && _curseCache.attached)
			{
				RunOnCached();
				return;
			}
			_curseCache = target.ability.GetInstance<CurseOfLight>();
			if (_curseCache != null)
			{
				_curseCache.Refresh();
			}
			else
			{
				target.ability.Add(_abilityComponent.ability);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void RunOnCached()
		{
			_curseCache.Refresh();
		}

		public override string ToString()
		{
			return this.GetAutoName();
		}
	}
}
