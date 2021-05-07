using Data;
using UnityEngine;

namespace Level
{
	[RequireComponent(typeof(Map))]
	public class SpecialMap : MonoBehaviour
	{
		public enum Type
		{
			Gauntlet,
			MysticalRuin,
			TimeCostChest,
			Treasure,
			TroopDefence
		}

		[SerializeField]
		[GetComponent]
		private Map _map;

		[SerializeField]
		private Type _type;

		public Map map => _map;

		public Type type => _type;

		public bool encountered
		{
			get
			{
				return GetEncoutered(_type);
			}
			set
			{
				SetEncoutered(_type, value);
			}
		}

		public static bool GetEncoutered(Type type)
		{
			return GameData.Progress.specialMapEncountered.GetData(type);
		}

		public static void SetEncoutered(Type type, bool value)
		{
			GameData.Progress.specialMapEncountered.SetData(type, value);
		}

		private void Awake()
		{
			encountered = true;
		}
	}
}
