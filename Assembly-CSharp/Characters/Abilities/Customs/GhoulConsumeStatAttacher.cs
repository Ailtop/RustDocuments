using System;
using Characters.Abilities.CharacterStat;
using UnityEditor;
using UnityEngine;

namespace Characters.Abilities.Customs
{
	public class GhoulConsumeStatAttacher : AbilityAttacher
	{
		[Serializable]
		private class KeyMap
		{
			[Serializable]
			public class Reorderable : ReorderableArray<KeyMap>
			{
			}

			public string key;

			[UnityEditor.Subcomponent(typeof(StackableStatBonusComponent))]
			public StackableStatBonusComponent statBonus;
		}

		[SerializeField]
		private MotionTypeBoolArray _motionTypeFilter;

		[SerializeField]
		private AttackTypeBoolArray _attackTypeFilter;

		[SerializeField]
		private CharacterTypeBoolArray _characterTypeFilter = new CharacterTypeBoolArray(true, true, true, true, true, false, false, false);

		[Space]
		[SerializeField]
		private KeyMap.Reorderable _keyMaps;

		public override void OnIntialize()
		{
		}

		public override void StartAttach()
		{
			Character character = base.owner;
			character.onKilled = (Character.OnKilledDelegate)Delegate.Combine(character.onKilled, new Character.OnKilledDelegate(OnOwnerKilled));
		}

		public override void StopAttach()
		{
			if (!(base.owner == null))
			{
				Character character = base.owner;
				character.onKilled = (Character.OnKilledDelegate)Delegate.Remove(character.onKilled, new Character.OnKilledDelegate(OnOwnerKilled));
				KeyMap[] values = _keyMaps.values;
				foreach (KeyMap keyMap in values)
				{
					base.owner.ability.Remove(keyMap.statBonus.ability);
				}
			}
		}

		private void OnOwnerKilled(ITarget target, ref Damage damage)
		{
			if (target.character == null || !_motionTypeFilter[damage.motionType] || !_attackTypeFilter[damage.attackType] || !_characterTypeFilter[target.character.type] || string.IsNullOrWhiteSpace(damage.key))
			{
				return;
			}
			KeyMap[] values = _keyMaps.values;
			foreach (KeyMap keyMap in values)
			{
				if (damage.key.Equals(keyMap.key, StringComparison.OrdinalIgnoreCase))
				{
					base.owner.ability.Add(keyMap.statBonus.ability);
					break;
				}
			}
		}
	}
}
