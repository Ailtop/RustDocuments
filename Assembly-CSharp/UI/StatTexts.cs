using System.Collections.Generic;
using Characters.Gear;
using Characters.Gear.Quintessences;
using Characters.Gear.Weapons;
using UnityEngine;

namespace UI
{
	public class StatTexts : MonoBehaviour
	{
		[SerializeField]
		private TextLayoutElement _statText;

		[SerializeField]
		private QuintessenceDesc _quintessenceDescription;

		[SerializeField]
		private SkillDesc _skillDescription;

		private Gear _gear;

		private void Awake()
		{
			_gear = GetComponentInParent<Gear>();
			SetSkillDescription();
			SetQuintessenceDescription();
			SetStatText(_gear.stat.strings);
		}

		private void SetSkillDescription()
		{
			Weapon weapon = _gear as Weapon;
			if (!(weapon == null))
			{
				for (int i = 0; i < weapon.currentSkills.Count; i++)
				{
					Object.Instantiate(_skillDescription, base.transform, false).info = weapon.currentSkills[i];
				}
			}
		}

		private void SetQuintessenceDescription()
		{
			Quintessence quintessence = _gear as Quintessence;
			if (!(quintessence == null))
			{
				Object.Instantiate(_quintessenceDescription, base.transform, false).text = quintessence.description;
			}
		}

		private void SetStatText(IList<string> _texts)
		{
			for (int i = 0; i < _texts.Count; i++)
			{
				Object.Instantiate(_statText, base.transform, false).text = _texts[i];
			}
		}
	}
}
