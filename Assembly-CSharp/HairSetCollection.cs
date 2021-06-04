using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Rust/Hair Set Collection")]
public class HairSetCollection : ScriptableObject
{
	[Serializable]
	public struct HairSetEntry
	{
		public HairSet HairSet;

		public GameObjectRef HairPrefab;

		public HairDyeCollection HairDyeCollection;
	}

	public HairSetEntry[] Head;

	public HairSetEntry[] Eyebrow;

	public HairSetEntry[] Facial;

	public HairSetEntry[] Armpit;

	public HairSetEntry[] Pubic;

	public HairSetEntry[] GetListByType(HairType hairType)
	{
		switch (hairType)
		{
		case HairType.Head:
			return Head;
		case HairType.Eyebrow:
			return Eyebrow;
		case HairType.Facial:
			return Facial;
		case HairType.Armpit:
			return Armpit;
		case HairType.Pubic:
			return Pubic;
		default:
			return null;
		}
	}

	public int GetIndex(HairSetEntry[] list, float typeNum)
	{
		return Mathf.Clamp(Mathf.FloorToInt(typeNum * (float)list.Length), 0, list.Length - 1);
	}

	public int GetIndex(HairType hairType, float typeNum)
	{
		HairSetEntry[] listByType = GetListByType(hairType);
		return GetIndex(listByType, typeNum);
	}

	public HairSetEntry Get(HairType hairType, float typeNum)
	{
		HairSetEntry[] listByType = GetListByType(hairType);
		return listByType[GetIndex(listByType, typeNum)];
	}
}
