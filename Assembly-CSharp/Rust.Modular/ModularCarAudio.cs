using UnityEngine;

namespace Rust.Modular;

public class ModularCarAudio : GroundVehicleAudio
{
	public bool showDebug;

	[Header("Skid")]
	[SerializeField]
	private SoundDefinition skidSoundLoop;

	[SerializeField]
	private SoundDefinition skidSoundDirtLoop;

	[SerializeField]
	private SoundDefinition skidSoundSnowLoop;

	[SerializeField]
	private float skidMinSlip = 10f;

	[SerializeField]
	private float skidMaxSlip = 25f;

	[Header("Movement & Suspension")]
	[SerializeField]
	private SoundDefinition movementStartOneshot;

	[SerializeField]
	private SoundDefinition movementStopOneshot;

	[SerializeField]
	private float movementStartStopMinTimeBetweenSounds = 0.25f;

	[SerializeField]
	private SoundDefinition movementRattleLoop;

	[SerializeField]
	private float movementRattleMaxSpeed = 10f;

	[SerializeField]
	private float movementRattleMaxAngSpeed = 10f;

	[SerializeField]
	private float movementRattleIdleGain = 0.3f;

	[SerializeField]
	private SoundDefinition suspensionLurchSound;

	[SerializeField]
	private float suspensionLurchMinExtensionDelta = 0.4f;

	[SerializeField]
	private float suspensionLurchMinTimeBetweenSounds = 0.25f;

	[Header("Wheels")]
	[SerializeField]
	private SoundDefinition tyreRollingSoundDef;

	[SerializeField]
	private SoundDefinition tyreRollingWaterSoundDef;

	[SerializeField]
	private SoundDefinition tyreRollingGrassSoundDef;

	[SerializeField]
	private SoundDefinition tyreRollingSnowSoundDef;

	[SerializeField]
	private AnimationCurve tyreRollGainCurve;
}
