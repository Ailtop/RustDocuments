using System;
using Characters.Controllers;
using Characters.Gear.Weapons;
using Services;
using UnityEngine;

namespace Characters.Operations
{
	public class StartWeaponPolymorph : CharacterOperation
	{
		[Serializable]
		private class SkillMap
		{
			[Serializable]
			public class Reorderable : ReorderableArray<SkillMap>
			{
			}

			public SkillInfo originalSkill;

			public string polymorphSkillKey;

			public bool copyCooldown;
		}

		[SerializeField]
		private Weapon _polymorphWeapon;

		[SerializeField]
		private SkillMap.Reorderable _skillMaps;

		private Weapon _weaponInstance;

		private void Awake()
		{
			_weaponInstance = UnityEngine.Object.Instantiate(_polymorphWeapon);
			_weaponInstance.transform.parent = null;
			_weaponInstance.name = _polymorphWeapon.name;
			_weaponInstance.Initialize();
			_weaponInstance.gameObject.SetActive(false);
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (!Service.quitting)
			{
				UnityEngine.Object.Destroy(_weaponInstance.gameObject);
			}
		}

		public override void Run(Character target)
		{
			target.playerComponents.inventory.weapon.Polymorph(_weaponInstance);
			ApplySkillMap();
		}

		private void ApplySkillMap()
		{
			if (_skillMaps.values.Length == 0)
			{
				return;
			}
			_weaponInstance.currentSkills.Clear();
			SkillMap[] values = _skillMaps.values;
			foreach (SkillMap skillMap in values)
			{
				SkillInfo[] skills = _weaponInstance.skills;
				foreach (SkillInfo skillInfo in skills)
				{
					if (skillMap.originalSkill.action.button != Button.None && skillInfo.key.Equals(skillMap.polymorphSkillKey, StringComparison.OrdinalIgnoreCase))
					{
						if (skillMap.copyCooldown)
						{
							skillInfo.action.cooldown.CopyCooldown(skillMap.originalSkill.action.cooldown);
						}
						_weaponInstance.currentSkills.Add(skillInfo);
						break;
					}
				}
			}
			_weaponInstance.SetSkillButtons();
		}
	}
}
