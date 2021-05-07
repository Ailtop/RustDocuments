using System;
using System.Collections;
using System.Text;
using UnityEngine;

namespace Characters.Gear.Synergy.Keywords
{
	public abstract class Keyword : MonoBehaviour
	{
		public enum Key
		{
			None,
			Madness,
			Swamp,
			Blast,
			FairyTale,
			Demolition,
			Duel,
			Fortress,
			Volcano,
			Execution,
			Necromancy,
			Sorcery,
			Fuse,
			Manatech,
			Alchemy,
			Soar,
			Fountain,
			Miser,
			Mirage,
			Heart,
			Mutation,
			Endurance,
			Tactics,
			Sprint,
			Static,
			Chase,
			March,
			Ruins,
			Leonia,
			Blizzard,
			Empire,
			Attitude,
			Adaptation,
			Brawl,
			Weakness,
			Blitz,
			Preparation
		}

		[NonSerialized]
		public int count;

		private int _level;

		public abstract Key key { get; }

		public int level
		{
			get
			{
				return _level;
			}
			protected set
			{
				if (_level != value)
				{
					if (value > maxLevel)
					{
						value = maxLevel;
					}
					int num = _level;
					_level = value;
					if (num == 0 && value > 0)
					{
						OnAttach();
					}
					if (num > 0 && value == 0)
					{
						OnDetach();
					}
				}
			}
		}

		protected abstract IList valuesByLevel { get; }

		public int maxLevel => valuesByLevel.Count - 1;

		public Character character { get; private set; }

		public static string GetName(Key key)
		{
			return Lingua.GetLocalizedString($"synergy/key/{key}/name");
		}

		public static Sprite GetIcon(Key key)
		{
			return Resource.instance.keywordIconDictionary[key.ToString()];
		}

		public virtual string GetDescription(Key key, int level)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<color=#B2977B>");
			for (int i = 1; i < valuesByLevel.Count; i++)
			{
				if (level == i)
				{
					stringBuilder.Append("<color=#755754>");
				}
				stringBuilder.Append(valuesByLevel[i]);
				if (level == i)
				{
					stringBuilder.Append("</color>");
				}
				if (i < valuesByLevel.Count - 1)
				{
					stringBuilder.Append('/');
				}
			}
			stringBuilder.Append("</color>");
			return string.Format(Lingua.GetLocalizedString($"synergy/key/{key}/desc"), stringBuilder.ToString());
		}

		public string GetCurrentDescription()
		{
			return GetDescription(key, level);
		}

		public string GetNextDescription()
		{
			return GetDescription(key, Mathf.Min(level + 1, maxLevel));
		}

		public void Initialize(Character character)
		{
			this.character = character;
			Initialize();
		}

		protected abstract void Initialize();

		public void UpdateLevel()
		{
			level = count;
			UpdateBonus();
		}

		protected abstract void UpdateBonus();

		protected abstract void OnAttach();

		protected abstract void OnDetach();
	}
}
