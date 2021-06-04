using System.Collections.Generic;
using UnityEngine;

public class VehicleLight : MonoBehaviour
{
	public bool IsBrake;

	public GameObject toggleObject;

	public Renderer lightRenderer;

	[Tooltip("Index of the material on the lightRenderer to modify emission on when lights turn on/off")]
	public int lightRendererMaterialIndex;

	[ColorUsage(true, true)]
	public Color lightOnColour;

	[ColorUsage(true, true)]
	public Color brakesOnColour;

	private static MaterialPropertyBlock materialPB;

	private static int emissionColorID = Shader.PropertyToID("_EmissionColor");

	public static void SetLightVisuals(IReadOnlyList<VehicleLight> lights, bool headlightsOn, bool brakesOn)
	{
		if (materialPB == null)
		{
			materialPB = new MaterialPropertyBlock();
		}
		foreach (VehicleLight light in lights)
		{
			if (light.toggleObject != null)
			{
				light.toggleObject.SetActive(headlightsOn);
			}
			if (light.lightRenderer != null)
			{
				Color value = (headlightsOn ? light.lightOnColour : ((!(light.IsBrake && brakesOn)) ? Color.black : light.brakesOnColour));
				materialPB.SetColor(emissionColorID, value);
				light.lightRenderer.SetPropertyBlock(materialPB, light.lightRendererMaterialIndex);
			}
		}
	}
}
