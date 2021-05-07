using System.Collections.Generic;
using System.Linq;
using Characters.Actions;
using UnityEngine;

namespace Characters.Gear.Weapons
{
	public class SkillInfo : MonoBehaviour
	{
		[SerializeField]
		private string _key;

		[SerializeField]
		private bool _hasAlways;

		[SerializeField]
		[Range(0f, 100f)]
		private int _weight = 1;

		public string key => _key;

		public bool hasAlways => _hasAlways;

		public int weight => _weight;

		public Sprite cachedIcon { get; private set; }

		public string displayName => Lingua.GetLocalizedString("skill/" + _key + "/name");

		public string description => Lingua.GetLocalizedString("skill/" + _key + "/desc");

		public Action action { get; private set; }

		public static SkillInfo WeightedRandomPop(List<SkillInfo> from)
		{
			int max = from.Sum((SkillInfo s) => s.weight);
			int num = Random.Range(0, max) + 1;
			for (int i = 0; i < from.Count; i++)
			{
				SkillInfo skillInfo = from[i];
				num -= skillInfo.weight;
				if (num <= 0)
				{
					from.RemoveAt(i);
					return skillInfo;
				}
			}
			return from[0];
		}

		public void Initialize()
		{
			action = GetComponent<Action>();
			cachedIcon = Resource.instance.GetSkillIcon(_key);
			if (cachedIcon == null)
			{
				Debug.LogError($"Couldn't find a skill icon file: {cachedIcon}.png");
			}
		}

		public Sprite GetIcon()
		{
			return Resource.instance.GetSkillIcon(_key);
		}
	}
}
