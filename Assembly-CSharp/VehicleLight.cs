using UnityEngine;

public class VehicleLight : MonoBehaviour, IClientComponent
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
}
