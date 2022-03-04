using UnityEngine;

public class FootstepSound : MonoBehaviour, IClientComponent
{
	public enum Hardness
	{
		Light = 1,
		Medium = 2,
		Hard = 3
	}

	public SoundDefinition lightSound;

	public SoundDefinition medSound;

	public SoundDefinition hardSound;

	private const float panAmount = 0.05f;
}
