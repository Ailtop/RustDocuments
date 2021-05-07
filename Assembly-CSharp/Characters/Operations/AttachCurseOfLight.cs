using System.Runtime.CompilerServices;
using Characters.Abilities;
using Characters.Abilities.Enemies;
using UnityEngine;

namespace Characters.Operations
{
	public class AttachCurseOfLight : TargetedCharacterOperation
	{
		[SerializeField]
		[AbilityComponent.Subcomponent]
		private AbilityComponent _abilityComponent;

		private IAbilityInstance _curseCache;

		public override void Initialize()
		{
			_abilityComponent.Initialize();
		}

		public override void Run(Character owner, Character target)
		{
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
