using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LifeInfographicStat : MonoBehaviour
{
	public enum DataType
	{
		None = 0,
		AliveTime_Short = 1,
		SleepingTime_Short = 2,
		KillerName = 3,
		KillerWeapon = 4,
		AliveTime_Long = 5,
		KillerDistance = 6,
		GenericStat = 7,
		DistanceTravelledWalk = 8,
		DistanceTravelledRun = 9,
		DamageTaken = 10,
		DamageHealed = 11,
		WeaponInfo = 12,
		SecondsWilderness = 13,
		SecondsSwimming = 14,
		SecondsInBase = 15,
		SecondsInMonument = 16,
		SecondsFlying = 17,
		SecondsBoating = 18,
		PlayersKilled = 19,
		ScientistsKilled = 20,
		AnimalsKilled = 21,
		SecondsDriving = 22
	}

	public enum WeaponInfoType
	{
		TotalShots = 0,
		ShotsHit = 1,
		ShotsMissed = 2,
		AccuracyPercentage = 3
	}

	public DataType dataSource;

	[Header("Generic Stats")]
	public string genericStatKey;

	[Header("Weapon Info")]
	public string targetWeaponName;

	public WeaponInfoType weaponInfoType;

	public TextMeshProUGUI targetText;

	public Image StatImage;
}
