using System;
using UnityEngine;

public class TrainEngineAudio : TrainCarAudio
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
	private Transform hornSoundPosition;

	[Header("Engine")]
	[SerializeField]
	private SoundDefinition engineStartSound;

	[SerializeField]
	private SoundDefinition engineStopSound;

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

	[Header("Horn")]
	[SerializeField]
	private SoundDefinition hornLoop;

	[SerializeField]
	private SoundDefinition hornStart;

	[Header("Other")]
	[SerializeField]
	private SoundDefinition lightsToggleSound;

	[SerializeField]
	private SoundDefinition proximityAlertDef;

	private Sound proximityAlertSound;

	[SerializeField]
	private SoundDefinition damagedLoopDef;

	private Sound damagedLoop;

	[SerializeField]
	private SoundDefinition changeThrottleDef;

	[SerializeField]
	private SoundDefinition changeCouplingDef;

	[SerializeField]
	private SoundDefinition unloadableStartDef;

	[SerializeField]
	private SoundDefinition unloadableEndDef;

	[SerializeField]
	private GameObject bellObject;

	[SerializeField]
	private SoundDefinition bellRingDef;

	[SerializeField]
	private SoundPlayer brakeSound;
}
