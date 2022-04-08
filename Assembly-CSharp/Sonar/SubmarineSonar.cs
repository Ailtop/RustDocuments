using UnityEngine;

namespace Sonar;

public class SubmarineSonar : FacepunchBehaviour
{
	[SerializeField]
	private float range = 100f;

	[SerializeField]
	private ParticleSystem sonarPS;

	[SerializeField]
	private ParticleSystem blipPS;

	[SerializeField]
	private SonarObject us;

	[SerializeField]
	private Color greenBlip;

	[SerializeField]
	private Color redBlip;

	[SerializeField]
	private Color whiteBlip;

	[SerializeField]
	private SubmarineAudio submarineAudio;
}
