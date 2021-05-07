using Characters.Gear.Synergy.Keywords;
using Services;
using UnityEngine;

namespace Characters.Gear.Synergy
{
	public class Synergy : MonoBehaviour
	{
		public readonly EnumArray<Keyword.Key, int> keywordCounts = new EnumArray<Keyword.Key, int>();

		public readonly EnumArray<Keyword.Key, Keyword> keywordComponents = new EnumArray<Keyword.Key, Keyword>();

		[SerializeField]
		private GameObject _container;

		public void Initialize(Character character)
		{
			Keyword[] componentsInChildren = _container.GetComponentsInChildren<Keyword>(true);
			foreach (Keyword keyword in componentsInChildren)
			{
				keywordComponents[keyword.key] = keyword;
				keyword.Initialize(character);
			}
		}

		public void UpdateBonus()
		{
			foreach (Keyword keywordComponent in keywordComponents)
			{
				if (!(keywordComponent == null))
				{
					keywordComponent.count = keywordCounts[keywordComponent.key];
					keywordComponent.UpdateLevel();
				}
			}
		}

		private void OnDestroy()
		{
			if (Service.quitting)
			{
				return;
			}
			foreach (Keyword keywordComponent in keywordComponents)
			{
				if (!(keywordComponent == null))
				{
					keywordComponent.count = 0;
					keywordComponent.UpdateLevel();
				}
			}
		}
	}
}
