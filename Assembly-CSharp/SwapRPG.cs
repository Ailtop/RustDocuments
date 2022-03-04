using System;
using UnityEngine;

public class SwapRPG : MonoBehaviour
{
	public enum RPGType
	{
		One = 0,
		Two = 1,
		Three = 2,
		Four = 3
	}

	public GameObject[] rpgModels;

	[NonSerialized]
	private string curAmmoType = "";

	public void SelectRPGType(int iType)
	{
		GameObject[] array = rpgModels;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(false);
		}
		rpgModels[iType].SetActive(true);
	}

	public void UpdateAmmoType(ItemDefinition ammoType)
	{
		if (!(curAmmoType == ammoType.shortname))
		{
			curAmmoType = ammoType.shortname;
			switch (curAmmoType)
			{
			default:
				SelectRPGType(0);
				break;
			case "ammo.rocket.fire":
				SelectRPGType(1);
				break;
			case "ammo.rocket.hv":
				SelectRPGType(2);
				break;
			case "ammo.rocket.smoke":
				SelectRPGType(3);
				break;
			}
		}
	}

	private void Start()
	{
	}
}
