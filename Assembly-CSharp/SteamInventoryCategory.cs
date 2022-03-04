using UnityEngine;

[CreateAssetMenu(menuName = "Rust/Generic Steam Inventory Category")]
public class SteamInventoryCategory : ScriptableObject
{
	public enum Price
	{
		CannotBuy = 0,
		VLV25 = 1,
		VLV50 = 2,
		VLV75 = 3,
		VLV100 = 4,
		VLV150 = 5,
		VLV200 = 6,
		VLV250 = 7,
		VLV300 = 8,
		VLV350 = 9,
		VLV400 = 10,
		VLV450 = 11,
		VLV500 = 12,
		VLV550 = 13,
		VLV600 = 14,
		VLV650 = 0xF,
		VLV700 = 0x10,
		VLV750 = 17,
		VLV800 = 18,
		VLV850 = 19,
		VLV900 = 20,
		VLV950 = 21,
		VLV1000 = 22,
		VLV1100 = 23,
		VLV1200 = 24,
		VLV1300 = 25,
		VLV1400 = 26,
		VLV1500 = 27,
		VLV1600 = 28,
		VLV1700 = 29,
		VLV1800 = 30,
		VLV1900 = 0x1F,
		VLV2000 = 0x20,
		VLV2500 = 33,
		VLV3000 = 34,
		VLV3500 = 35,
		VLV4000 = 36,
		VLV4500 = 37,
		VLV5000 = 38,
		VLV6000 = 39,
		VLV7000 = 40,
		VLV8000 = 41,
		VLV9000 = 42,
		VLV10000 = 43
	}

	public enum DropChance
	{
		NeverDrop = 0,
		VeryRare = 1,
		Rare = 2,
		Common = 3,
		VeryCommon = 4,
		ExtremelyRare = 5
	}

	[Header("Steam Inventory")]
	public bool canBeSoldToOtherUsers;

	public bool canBeTradedWithOtherUsers;

	public bool isCommodity;

	public Price price;

	public DropChance dropChance;

	public bool CanBeInCrates = true;
}
