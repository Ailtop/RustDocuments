using UnityEngine;

public abstract class GroundVehicleAudio : MonoBehaviour, IClientComponent
{
	[SerializeField]
	protected GroundVehicle groundVehicle;

	[Header("Engine")]
	[SerializeField]
	private SoundDefinition engineStartSound;

	[SerializeField]
	private SoundDefinition engineStopSound;

	[SerializeField]
	private SoundDefinition engineStartFailSound;

	[SerializeField]
	private BlendedLoopEngineSound blendedEngineLoops;

	[SerializeField]
	private float wheelRatioMultiplier = 600f;

	[Header("Water")]
	[SerializeField]
	private SoundDefinition waterSplashSoundDef;

	[SerializeField]
	private BlendedSoundLoops waterLoops;

	[SerializeField]
	private float waterSoundsMaxSpeed = 10f;

	[SerializeField]
	[Header("Brakes")]
	private SoundDefinition brakeSoundDef;

	[SerializeField]
	[Header("Lights")]
	private SoundDefinition lightsToggleSound;
}
