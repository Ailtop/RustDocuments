using TMPro;
using UnityEngine;

public class VitalNote : MonoBehaviour, IClientComponent, IVitalNotice
{
	public enum Vital
	{
		Comfort = 0,
		Radiation = 1,
		Poison = 2,
		Cold = 3,
		Bleeding = 4,
		Hot = 5,
		Oxygen = 6,
		Wet = 7,
		Hygiene = 8,
		Starving = 9,
		Dehydration = 10
	}

	public Vital VitalType;

	public FloatConditions showIf;

	public TextMeshProUGUI valueText;
}
