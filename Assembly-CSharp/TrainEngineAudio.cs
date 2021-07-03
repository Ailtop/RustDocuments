using System;
using UnityEngine;

public class TrainEngineAudio : MonoBehaviour
{
	[Serializable]
	public class EngineReflection
	{
		public Vector3 direction;

		public Vector3 offset;

		public SoundDefinition soundDef;

		public Sound sound;

		public SoundModulation.Modulator pitchMod;

		public SoundModulation.Modulator gainMod;

		public float distance = 20f;
	}

	[SerializeField]
	private TrainEngine trainEngine;

	[SerializeField]
	private Transform cockpitSoundPosition;

	[SerializeField]
	[Header("Engine")]
	private SoundDefinition engineStartSound;

	[SerializeField]
	private SoundDefinition engineStopSound;

	[SerializeField]
	private SoundDefinition engineStartFailSound;

	[SerializeField]
	private SoundDefinition engineActiveLoopDef;

	[SerializeField]
	private AnimationCurve engineActiveLoopPitchCurve;

	[SerializeField]
	private float engineActiveLoopChangeSpeed = 0.2f;

	private Sound engineActiveLoop;

	private SoundModulation.Modulator engineActiveLoopPitch;

	[SerializeField]
	private BlendedLoopEngineSound engineLoops;

	[SerializeField]
	private EngineReflection[] engineReflections;

	[SerializeField]
	private LayerMask reflectionLayerMask;

	[SerializeField]
	private float reflectionMaxDistance = 20f;

	[SerializeField]
	private float reflectionGainChangeSpeed = 10f;

	[SerializeField]
	private float reflectionPositionChangeSpeed = 10f;

	[SerializeField]
	private float reflectionRayOffset = 0.5f;

	[Header("Other")]
	[SerializeField]
	private SoundDefinition lightsToggleSound;

	[SerializeField]
	private SoundDefinition proximityAlertDef;

	private Sound proximityAlertSound;

	[SerializeField]
	private SoundDefinition movementStartDef;

	[SerializeField]
	private SoundDefinition movementStopDef;

	[SerializeField]
	private SoundDefinition movementLoopDef;

	[SerializeField]
	private AnimationCurve movementLoopGainCurve;

	[SerializeField]
	private float movementChangeOneshotDebounce = 1f;

	private Sound movementLoop;

	private SoundModulation.Modulator movementLoopGain;

	[SerializeField]
	private SoundDefinition turnLoopDef;

	private Sound turnLoop;

	[SerializeField]
	private SoundDefinition trackClatterLoopDef;

	[SerializeField]
	private AnimationCurve trackClatterGainCurve;

	[SerializeField]
	private AnimationCurve trackClatterPitchCurve;

	private Sound trackClatterLoop;

	private SoundModulation.Modulator trackClatterGain;

	private SoundModulation.Modulator trackClatterPitch;

	[SerializeField]
	private SoundDefinition damagedLoopDef;

	private Sound damagedLoop;

	[SerializeField]
	private SoundDefinition changeThrottleDef;

	[SerializeField]
	private SoundPlayer brakeSound;
}
