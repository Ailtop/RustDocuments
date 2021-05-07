using Characters.Gear.Weapons;
using UnityEngine;

namespace UI.Inventory
{
	public class SkillOptions : MonoBehaviour
	{
		[SerializeField]
		private GameObject optionContainer;

		[SerializeField]
		private SkillOption option;

		[SerializeField]
		private GameObject option2Container;

		[SerializeField]
		private SkillOption optionLeft;

		[SerializeField]
		private SkillOption optionRight;

		public void Set(Weapon weapon)
		{
			if (weapon.currentSkills.Count == 1)
			{
				optionContainer.SetActive(true);
				option2Container.SetActive(false);
				option.Set(weapon.currentSkills[0]);
			}
			else
			{
				optionContainer.SetActive(false);
				option2Container.SetActive(true);
				optionLeft.Set(weapon.currentSkills[0]);
				optionRight.Set(weapon.currentSkills[1]);
			}
		}
	}
}
