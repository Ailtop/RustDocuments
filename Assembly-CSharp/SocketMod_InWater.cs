using UnityEngine;

public class SocketMod_InWater : SocketMod
{
	public bool wantsInWater = true;

	public static Translate.Phrase WantsWaterPhrase = new Translate.Phrase("error_inwater_wants", "Must be placed in water");

	public static Translate.Phrase NoWaterPhrase = new Translate.Phrase("error_inwater", "Can't be placed in water");

	private void OnDrawGizmosSelected()
	{
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.color = Color.cyan;
		Gizmos.DrawSphere(Vector3.zero, 0.1f);
	}

	public override bool DoCheck(Construction.Placement place)
	{
		if (WaterLevel.Test(place.position + place.rotation * worldPosition - new Vector3(0f, 0.1f, 0f), waves: true, volumes: true) == wantsInWater)
		{
			return true;
		}
		if (wantsInWater)
		{
			Construction.lastPlacementError = WantsWaterPhrase.translated;
		}
		else
		{
			Construction.lastPlacementError = NoWaterPhrase.translated;
		}
		return false;
	}
}
