using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VitalInfo : MonoBehaviour, IClientComponent, IVitalNotice
{
	public enum Vital
	{
		BuildingBlocked = 0,
		CanBuild = 1,
		Crafting = 2,
		CraftLevel1 = 3,
		CraftLevel2 = 4,
		CraftLevel3 = 5,
		DecayProtected = 6,
		Decaying = 7,
		SafeZone = 8,
		Buffed = 9,
		Pet = 10,
		ModifyClan = 11
	}

	public HudElement Element;

	public Image InfoImage;

	public Vital VitalType;

	public TextMeshProUGUI text;
}
